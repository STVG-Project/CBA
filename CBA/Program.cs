using Microsoft.EntityFrameworkCore;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using CBA.Models;
using CBA.APIs;
using static CBA.APIs.MyPerson;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using static CBA.APIs.MyFace;

namespace CBA;

public class Program
{
    public class ItemHost
    {
        public List<string> host { get; set; } = new List<string>();
    }

    public class HttpNotification
    {
        public string id { get; set; } = "";
        public List<string> messagers { get; set; } = new List<string>();
    }

    
    public class CacheForUser
    {
        public string id { get; set; } = "";
        public ListPersonPage? dataPersons { get; set; } = null;
        public CacheHistoryPerson dataHistory { get; set; } = new CacheHistoryPerson();
        public bool flag { get; set; } = false;
        
    }


    public static MyRole api_role = new MyRole();
    public static MyUser api_user = new MyUser();
    public static MyFile api_file = new MyFile();
    public static MyDevice api_device = new MyDevice();
    public static MyGroup api_group = new MyGroup();
    public static MyPerson api_person = new MyPerson();
    public static MyFace api_face = new MyFace();
    public static MyReport api_report = new MyReport();
    public static MyAgeLevel api_age = new MyAgeLevel();
    public static List<HttpNotification> httpNotifications = new List<HttpNotification>();
    public static List<CacheForUser> caches { get; set; } = new List<CacheForUser>();

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console(theme: AnsiConsoleTheme.Code)
               .WriteTo.File("mylog.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();
        try
        {
            string path = "./Configs";
            string link = Path.Combine(path, "configSql.json");

            if (!File.Exists(link))
            {
                bool flag = Program.api_file.createConfig("configSql");
                if (!flag)
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("DB null !!! ");
                    }
                }
            }

            string file = Program.api_file.getFileConfig();

            ItemHost? tmp = JsonConvert.DeserializeObject<ItemHost>(file);
            if (tmp != null)
            {
                if (tmp.host.Count > 0)
                {
                    DataContext.configSql = tmp.host[0];
                }
            }

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel((context, option) =>
            {
                option.ListenAnyIP(50000, listenOptions =>
                {

                });
                option.Limits.MaxConcurrentConnections = null;
                option.Limits.MaxRequestBodySize = null;
                option.Limits.MaxRequestBufferSize = null;
            });
            // Add services to the container.
            //builder.Logging.AddSerilog();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("HTTPSystem", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).WithExposedHeaders("Grpc-Status", "Grpc-Encoding", "Grpc-Accept-Encoding");
                });
            });

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(DataContext.configSql));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                DataContext datacontext = services.GetRequiredService<DataContext>();
                datacontext.Database.EnsureCreated();
                await datacontext.Database.MigrateAsync();

            }

            int index = 1;

            Log.Information(String.Format("Connected to Server {0}_v{1} : {2} " , DateTime.Now, index.ToString() , DataContext.configSql));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();

            app.UseCors("HTTPSystem");
            app.UseRouting();

            app.UseAuthorization();


            app.MapControllers();
            app.MapGet("/", () => string.Format("CBA of STVG - {0}", DateTime.Now));
            await api_role.initAsync();
            await api_user.initAsync();
            await api_group.initAsync();
            api_file.initCreateTargetFile();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }

        Log.CloseAndFlush();
    }
}
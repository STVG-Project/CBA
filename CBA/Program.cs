using Microsoft.EntityFrameworkCore;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using CBA.Models;
using CBA.APIs;
using static CBA.APIs.MyPerson;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;

namespace CBA;

public class Program
{
    public class HttpNotification
    {
        public string group { get; set; } = "";
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


    public class ItemHost
    {
        public List<string> host { get; set; } = new List<string>();
    }
    public static List<CacheForUser> caches { get; set; } = new List<CacheForUser>();

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Console(theme: AnsiConsoleTheme.Code)
               .WriteTo.File("mylog.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();
        string path = "./Configs";
        /*if (!Directory.Exists(path))
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
        else
        {
            string link = path + "configSql.json";
            if(!File.Exists(link))
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
        }*/
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
                foreach (string item in tmp.host)
                {
                    DataContext.configSql = item;
                    string ip = "";
                    try
                    {
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

                        string host = DataContext.configSql.Split(";")[0];
                        ip = host.Split("=")[1];


                        var app = builder.Build();

                        using (var scope = app.Services.CreateScope())
                        {
                            IServiceProvider services = scope.ServiceProvider;
                            DataContext datacontext = services.GetRequiredService<DataContext>();
                            datacontext.Database.EnsureCreated();
                            await datacontext.Database.MigrateAsync();

                        }


                        Log.Information("Connected to Server : " + DataContext.configSql);

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

                        app.Run();
                    }
                    catch (Exception ex)
                    {
                        if(ex.Message.CompareTo(string.Format("Failed to connect to {0}", ip)) == 0)
                        {
                            continue;
                        }   
                        else
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }
            }
        }
        Log.CloseAndFlush();
    }
}
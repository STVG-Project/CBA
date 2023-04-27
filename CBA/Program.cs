using Microsoft.EntityFrameworkCore;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using CBA.Models;
using CBA.APIs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static CBA.APIs.MyPerson;

namespace CBA;

public class Program
{

    public static MyRole api_role = new MyRole();
    public static MyUser api_user = new MyUser();
    public static MyFile api_file = new MyFile();
    public static MyDevice api_device = new MyDevice();
    public static MyGroup api_group = new MyGroup();
    public static MyPerson api_person = new MyPerson();
    public static MyFace api_face = new MyFace();
    public static MyReport api_report = new MyReport();
    public static MyAgeLevel api_age = new MyAgeLevel();
   
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Console(theme: AnsiConsoleTheme.Code)
               .WriteTo.File("mylog.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();
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

            using (StreamReader sr = new StreamReader("Config.txt"))
            {
                string? line = sr.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    DataContext.configSql = line;
                }
            }
            Log.Information("Connected to Server : " + DataContext.configSql);

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(DataContext.configSql));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
            using (var scope = app.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                DataContext datacontext = services.GetRequiredService<DataContext>();
                datacontext.Database.EnsureCreated();
                await datacontext.Database.MigrateAsync();
            }

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
            Log.Error(ex.ToString());
        }
        Log.CloseAndFlush();
    }
}
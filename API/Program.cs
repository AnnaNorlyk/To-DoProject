using Serilog;
using Monitoring;
using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;
using FeatureHubSDK;
using FeatureFlags;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitorService.Initialize();
            var logger = MonitorService.Log;

            try
            {
                logger.Information("Starting up...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Logging.ClearProviders();

                builder.Configuration
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();

                builder.Host.UseSerilog((ctx, lc) =>
                    lc.ReadFrom.Configuration(ctx.Configuration)
                      .Enrich.FromLogContext()
                );


                logger.Information("Current environment: {Env}", builder.Environment.EnvironmentName);

                // Register services
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowFrontend", policy =>
                    {
                        policy.WithOrigins(
                            "http://localhost:3000",
                            "http://141.147.1.249:3000"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
                });


                var dbHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
                var dbName = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
                var dbUser = Environment.GetEnvironmentVariable("MYSQL_USER");
                var dbPass = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

                if (string.IsNullOrWhiteSpace(dbHost) ||
                    string.IsNullOrWhiteSpace(dbName) ||
                    string.IsNullOrWhiteSpace(dbUser) ||
                    string.IsNullOrWhiteSpace(dbPass))
                {
                    throw new InvalidOperationException("environment variables are missing.");
                }

                var connectionString = $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPass};";

                builder.Services.AddDbContext<TodoContext>(options =>
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6)))
                );


                FeatureFlagInitializer.Configure(builder.Services);


                builder.Services.AddScoped<ITodoListService, TodoListService>();

                var app = builder.Build();


                app.UseSwagger();
                app.UseSwaggerUI(c => c.RoutePrefix = "");

                app.UseCors("AllowFrontend");
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Application failed to start correctly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

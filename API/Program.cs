using Serilog;
using Monitoring;
using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;
using FeatureHubSDK;

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

                // Remove default providers so only Serilog is used
                builder.Logging.ClearProviders();

                // Load appsettings for Serilog
                builder.Configuration
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();

                // Tell the host to use Serilog
                builder.Host.UseSerilog((ctx, lc) =>
                    lc.ReadFrom.Configuration(ctx.Configuration)
                      .Enrich.FromLogContext()
                );

                // Register FeatureHub
                var edgeUrl = Environment.GetEnvironmentVariable("FEATUREHUB_EDGE_URL")
                              ?? throw new InvalidOperationException("FEATUREHUB_EDGE_URL missing in environment");
                var apiKey = Environment.GetEnvironmentVariable("FEATUREHUB_API_KEY")
                              ?? throw new InvalidOperationException("FEATUREHUB_API_KEY missing in environment");


                var featureHubContext = new EdgeFeatureHubConfig(edgeUrl, apiKey)
                                          .NewContext()
                                          .Build()
                                          .GetAwaiter()
                                          .GetResult();

                builder.Services.AddSingleton<IClientContext>(_ => featureHubContext);

                // Services
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

                // Database
                var dbHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
                var dbName = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
                var dbUser = Environment.GetEnvironmentVariable("MYSQL_USER");
                var dbPass = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

                var connectionString = $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPass};";
                builder.Services.AddDbContext<TodoContext>(options =>
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6))));

                builder.Services.AddScoped<ITodoListService, TodoListService>();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseCors("AllowFrontend");
                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                // If startup fails, log fatal
                logger.Fatal(ex, "Application failed to start correctly");
                throw;
            }
            finally
            {
                // Ensure any buffered events are flushed
                Log.CloseAndFlush();
            }
        }
    }
}

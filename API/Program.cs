using Serilog;
using Monitoring;
using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;

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

                builder.Logging.ClearProviders(); // Removes default providers
                builder.Host.UseSerilog(logger);

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

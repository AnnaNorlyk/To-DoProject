using Serilog;
using Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitorService.Initialize();
            
            var logger = MonitorService.Log;

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(logger);

            logger.Information("Seq sink is now configured and ready to receive events");


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

            // Build connection string from environment variables
            var dbHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
            var dbName = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
            var dbUser = Environment.GetEnvironmentVariable("MYSQL_USER");
            var dbPass = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

            var connectionString = $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPass};";

            // Use the constructed connection string
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
    }
}

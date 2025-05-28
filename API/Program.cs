using Serilog;
using Serilog.Core;
using Serilog.Events;
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

            var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);

            try
            {
                logger.Information("Starting up...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Configuration
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();

                builder.Logging.ClearProviders();

                builder.Host.UseSerilog((ctx, lc) =>
                    lc
                     .MinimumLevel.ControlledBy(levelSwitch)
                     .Enrich.FromLogContext()
                     .WriteTo.Console()
                     .WriteTo.Seq(
                         serverUrl: ctx.Configuration["Seq__ServerUrl"],
                         controlLevelSwitch: levelSwitch,
                         restrictedToMinimumLevel: LogEventLevel.Information,
                         durable: true
                     )
                );

                IClientContext featureHubContext = null;
                var edgeUrl = Environment.GetEnvironmentVariable("FEATUREHUB_URL");
                var apiKey = Environment.GetEnvironmentVariable("FEATUREHUB_API_KEY");
                if (!string.IsNullOrEmpty(edgeUrl) && !string.IsNullOrEmpty(apiKey))
                {
                    try
                    {
                        featureHubContext = new EdgeFeatureHubConfig(edgeUrl, apiKey)
                                              .NewContext()
                                              .Build()
                                              .GetAwaiter()
                                              .GetResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "FeatureHub init failed for {Url}", edgeUrl);
                    }
                }
                else
                {
                    Log.Warning("FEATUREHUB_URL or FEATUREHUB_API_KEY not set, using no-op client");
                }

                var app = builder.Build();

                builder.Services.AddSingleton<IClientContext>(_ => featureHubContext);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddCors(options =>
                {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(
                            "http://localhost:3000",

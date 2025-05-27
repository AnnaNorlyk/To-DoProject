using Serilog;
using Serilog.Enrichers.Span;
using System;
using System.Reflection;

namespace Monitoring
{
    public static class MonitorService
    {
        public static readonly string ServiceName =
            Assembly.GetCallingAssembly().GetName().Name ?? "Unknown";


        //Expose the configured Serilog logger.

        public static ILogger Log => Serilog.Log.Logger;

        public static void Initialize()
        {
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL")
                         ?? throw new InvalidOperationException("SEQ_URL environment variable is not set.");

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // general level
                .MinimumLevel.Override("API.Services", LogEventLevel.Debug) //allows more detailed logging for specific namespaces
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.Seq(serverUrl: seqUrl)
                .Enrich.WithSpan()
                .CreateLogger();

        }
    }
}

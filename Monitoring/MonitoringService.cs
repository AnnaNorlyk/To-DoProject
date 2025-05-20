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

        static MonitorService()
        {
            // Enable  Serilog diagnostics
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.Seq(serverUrl: "http://localhost:5341") //"http://seq:5341")
                .Enrich.WithSpan()
                .CreateLogger();
        }
    }
}

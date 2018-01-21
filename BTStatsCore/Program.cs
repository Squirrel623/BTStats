using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

using BTStatsCorePopulator;
using Serilog.Events;

namespace BTStatsCore
{
    public class Program
    {
        public static int Main(string[] args)
        {
            string logDir = Environment.GetEnvironmentVariable("ServerLogsDir");
            if (string.IsNullOrEmpty(logDir))
            {
                logDir = ".";
            }

            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                 .Enrich.FromLogContext()
                 .WriteTo.File(Path.Combine(logDir,"log.txt"), rollingInterval: RollingInterval.Month, flushToDiskInterval: TimeSpan.FromMinutes(1))
                 .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                BuildWebHost(args).Run();
                return 0;
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.Information("Stopping web host");
                Log.CloseAndFlush();
            }
            
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseSerilog()
                .UseUrls("http://*:80")
                .Build();
    }
}

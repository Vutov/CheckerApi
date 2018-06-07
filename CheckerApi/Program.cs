using System;
using System.IO;
using System.Threading.Tasks;
using CheckerApi.Context;
using CheckerApi.Extensions;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace CheckerApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            File.WriteAllText("./version.txt", $"{DateTime.UtcNow:G}");
            var host = BuildWebHost(args);
            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApiContext>();
                context.Seed();

            }

            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(30)).Wait();
                    using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var syncService = serviceScope.ServiceProvider.GetRequiredService<ISyncService>();
                        syncService.Run();
                    }
                }
            });

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    var logLevel = hostingContext.HostingEnvironment.IsDevelopment()
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Error;

                    loggerConfiguration
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                        .Enrich.WithProperty("HostName", Environment.MachineName)
                        .WriteTo.Console(theme: SystemConsoleTheme.Literate, restrictedToMinimumLevel: logLevel)
                        .WriteTo.File("./errorlogs.txt", LogEventLevel.Error);

                    SelfLog.Enable(Console.Error);
                })
                // TODO Check if exists
                .UseKestrel(options => options.ConfigureEndpoints())
                .Build();
    }
}

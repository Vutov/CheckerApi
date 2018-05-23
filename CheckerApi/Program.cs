using System;
using System.Threading.Tasks;
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
                    Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                    using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var context = serviceScope.ServiceProvider.GetService<ApiContext>();
                        var syncService = serviceScope.ServiceProvider.GetRequiredService<Sync>();
                        syncService.Run(context);
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
                    loggerConfiguration
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                        .Enrich.WithProperty("HostName", Environment.MachineName)
                        .WriteTo.Console(theme: SystemConsoleTheme.Literate)
                        .WriteTo.File("./errorlogs.txt", LogEventLevel.Error);

                    SelfLog.Enable(Console.Error);
                })
                .Build();
    }
}

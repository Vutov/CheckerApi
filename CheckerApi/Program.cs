using System;
using CheckerApi.Extensions;
using CheckerApi.Jobs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
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
            BuildWebHost(args)
                .CreateVersionFile()
                .SeedDatabase()
                .SetupScheduler((scheduler, host) =>
                {
                    scheduler.AddJob<SyncJob>(
                        host,
                        tb => tb.WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(30)
                            .RepeatForever()
                        ),
                        startAt: DateTimeOffset.UtcNow.AddSeconds(15)
                    ).AddJob<CleanerJob>(
                        host,
                        tb => tb.WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(30)
                            .RepeatForever()
                        ),
                        startAt: DateTimeOffset.UtcNow.AddSeconds(30)
                    ).AddJob<ZipJob>(
                        host,
                        tb => tb.WithSimpleSchedule(x => x
                            .WithIntervalInHours(24)
                            .RepeatForever()
                        ),
                        startAt: DateTime.UtcNow.EndOfDay()
                    );

                    using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var config = serviceScope.ServiceProvider.GetService<IConfiguration>();
                        var pool = config.GetValue<string>("Pool:Url");
                        if (!string.IsNullOrEmpty(pool))
                        {
                            scheduler.AddJob<NetworkHashrateJob>(
                                host,
                                tb => tb.WithSimpleSchedule(x => x
                                    .WithIntervalInMinutes(5)
                                    .RepeatForever()
                                ),
                                startAt: DateTimeOffset.UtcNow.AddSeconds(5)
                            );
                        }
                    }
                })
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .MinimumLevel.Verbose()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                        .Enrich.WithProperty("HostName", Environment.MachineName)
                        .WriteTo.Console(theme: SystemConsoleTheme.Literate);

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        loggerConfiguration.WriteTo.File("./errorlogs.txt", LogEventLevel.Error);
                    }

                    SelfLog.Enable(Console.Error);
                })
                .UseKestrel()
                .Build();
    }
}

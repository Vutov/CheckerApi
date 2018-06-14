using System;
using System.IO;
using CheckerApi.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace CheckerApi.Extensions
{
    public static class WebHostExtensions
    {
        public static IWebHost SetupScheduler(this IWebHost host, Action<IScheduler, IWebHost> jobs)
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler scheduler = schedFact.GetScheduler().Result;
            jobs(scheduler, host);
            scheduler.Start();

            return host;
        }

        public static IWebHost CreateVersionFile(this IWebHost host)
        {
            File.WriteAllText("./version.txt", $"{DateTime.UtcNow:G}");
            return host;
        }

        public static IWebHost SeedDatabase(this IWebHost host)
        {
            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApiContext>();
                context.Seed();
            }

            return host;
        }
    }
}

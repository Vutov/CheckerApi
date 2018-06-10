using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CheckerApi.Jobs
{
    public abstract class Job : IJob
    {
        public abstract void Execute(JobDataMap data, IServiceProvider serviceProvider);

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                var host = (IWebHost)context.MergedJobDataMap["host"];

                using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var logger = serviceProvider.GetService<ILogger<Job>>();

                    try
                    {
                        Execute(context.MergedJobDataMap, serviceProvider);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical($"Error executing Job {ex}");
                    }
                }
            });
        }
    }
}

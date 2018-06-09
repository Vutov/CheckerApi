using System;
using System.Threading.Tasks;
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
                var serviceProvider = (IServiceProvider)context.MergedJobDataMap["serviceProvider"];
                var logger = serviceProvider.GetService<ILogger<Job>>();

                try
                {
                    Execute(context.MergedJobDataMap, serviceProvider);
                }
                catch (Exception ex)
                {
                    logger.LogCritical($"Error executing Job {ex}");
                }
            });
        }
    }
}

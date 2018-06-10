using System;
using Microsoft.AspNetCore.Hosting;
using Quartz;

namespace CheckerApi.Extensions
{
    public static class SchedulerExtensions
    {
        public static IScheduler AddJob<T>(this IScheduler scheduler, IWebHost host, Func<TriggerBuilder, TriggerBuilder> triggerFunc) where T : IJob
        {
            return scheduler.AddJob<T>(host, j => j, triggerFunc);
        }

        public static IScheduler AddJob<T>(this IScheduler scheduler, IWebHost host, Func<IJobDetail, IJobDetail> jobFunc, Func<TriggerBuilder, TriggerBuilder> triggerFunc) where T : IJob
        {
            var guid = Guid.NewGuid().ToString();
            var job = jobFunc(JobBuilder.Create<T>()
                .WithIdentity(guid)
                .Build());
            job.JobDataMap["host"] = host;

            var triggerBuilder = triggerFunc(TriggerBuilder.Create()
                .WithIdentity(guid)
                .StartNow());
            var trigger = triggerBuilder.Build();

            scheduler.ScheduleJob(job, trigger);
            return scheduler;
        }
    }
}

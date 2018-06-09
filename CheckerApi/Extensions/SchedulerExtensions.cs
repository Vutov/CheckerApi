using System;
using Quartz;

namespace CheckerApi.Extensions
{
    public static class SchedulerExtensions
    {
        public static IScheduler AddJob<T>(this IScheduler scheduler, IServiceProvider serviceProvider, Func<TriggerBuilder, TriggerBuilder> triggerFunc) where T : IJob
        {
            return scheduler.AddJob<T>(serviceProvider, j => j, triggerFunc);
        }

        public static IScheduler AddJob<T>(this IScheduler scheduler, IServiceProvider serviceProvider, Func<IJobDetail, IJobDetail> jobFunc, Func<TriggerBuilder, TriggerBuilder> triggerFunc) where T : IJob
        {
            var job = jobFunc(JobBuilder.Create<T>()
                .WithIdentity(typeof(T).Name)
                .Build());
            job.JobDataMap["serviceProvider"] = serviceProvider;

            var triggerBuilder = triggerFunc(TriggerBuilder.Create()
                .WithIdentity(typeof(T).Name)
                .StartNow());
            var trigger = triggerBuilder.Build();

            scheduler.ScheduleJob(job, trigger);
            return scheduler;
        }
    }
}

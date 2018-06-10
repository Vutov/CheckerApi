using System;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<ISyncService>();
            service.Run();
        }
    }
}

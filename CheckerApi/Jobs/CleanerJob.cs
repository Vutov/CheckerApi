using System;
using System.Linq;
using CheckerApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class CleanerJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<CleanerJob>>();
            logger.LogInformation("Clean Started");

            var recordThreshold = TimeSpan.FromMinutes(config.GetValue<int>("Api:ClearAuditMinutes"));
            var context = serviceProvider.GetService<ApiContext>();
            var time = DateTime.UtcNow.Add(-recordThreshold);
           
            var toClean = context
                .OrdersAudit
                .Where(o => o.RecordDate <= time)
                .ToList();

            context.OrdersAudit.RemoveRange(toClean);
            context.SaveChanges();

            logger.LogInformation("Clean Ended");
        }
    }
}

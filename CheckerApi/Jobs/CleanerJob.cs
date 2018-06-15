using System;
using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();

            var recordThreshold = TimeSpan.FromMinutes(config.GetValue<int>("Api:ClearAuditMinutes"));
            var context = serviceProvider.GetService<ApiContext>();
            var time = DateTime.UtcNow.Add(-recordThreshold);

            context.Database.ExecuteSqlCommand(new RawSqlString($"DELETE FROM OrderAudits WHERE RecordDate <= '{time:s}'"));
            context.SaveChanges();

            sw.Stop();
            var elapsed = sw.Elapsed;
            logger.LogInformation($"Clean Ended in {elapsed.TotalSeconds} sec");
        }
    }
}

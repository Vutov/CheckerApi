using System;
using CheckerApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class CleanerJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();

            var recordThreshold = TimeSpan.FromMinutes(config.GetValue<int>("Api:ClearAuditMinutes"));
            var context = serviceProvider.GetService<ApiContext>();
            var auditTime = DateTime.UtcNow.Add(-recordThreshold);

            context.Database.ExecuteSqlCommand(new RawSqlString("DELETE FROM OrderAudits WHERE RecordDate <= @p0"), $"{auditTime:s}");
            context.SaveChanges();

            var hashrateThreshold = TimeSpan.FromMinutes(config.GetValue<int>("Monitor:StoreForMinutes"));
            var hashrateTime = DateTime.UtcNow.Add(-hashrateThreshold);
            context.Database.ExecuteSqlCommand(new RawSqlString("DELETE FROM PoolHashrate WHERE EntryDate <= @p0"), $"{hashrateTime:s}");
            context.SaveChanges();
        }
    }
}

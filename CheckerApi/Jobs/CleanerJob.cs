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
            var time = DateTime.UtcNow.Add(-recordThreshold);

            context.Database.ExecuteSqlCommand(new RawSqlString("DELETE FROM OrderAudits WHERE RecordDate <= @p0"), $"{time:s}");
            context.SaveChanges();
        }
    }
}

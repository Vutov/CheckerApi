using System;
using System.IO;
using System.Linq;
using CheckerApi.Context;
using CheckerApi.Extensions;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using ServiceStack;

namespace CheckerApi.Jobs
{
    public class ZipJob: Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var compressor = serviceProvider.GetService<ICompressService>();
            var context = serviceProvider.GetService<ApiContext>();

            var yesterday = DateTime.UtcNow.AddDays(-1);
            var start = yesterday.StartOfDay();
            var end = yesterday.EndOfDay();

            var audits = context.OrdersAuditsReadOnly
                .Where(o => o.RecordDate >= start)
                .Where(o => o.RecordDate <= end)
                .OrderByDescending(o => o.RecordDate)
                .ToList()
                .ToCsv();
            var zip = compressor.Zip(audits, $"{yesterday:yyyyMMdd}.csv");

            Directory.CreateDirectory("./AuditZips"); // If the directory already exists, this method does nothing.
            File.WriteAllBytes($"./AuditZips/{yesterday:yyyyMMdd}.zip", zip);
        }
    }
}

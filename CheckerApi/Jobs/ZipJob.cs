using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CheckerApi.Context;
using CheckerApi.Extensions;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var logger = serviceProvider.GetService<ILogger<ZipJob>>();
            var sw = Stopwatch.StartNew();

            var yesterday = DateTime.UtcNow.AddDays(-1);
            var start = yesterday.StartOfDay();
            var end = yesterday.EndOfDay();

            logger.LogInformation($"ZipJob begin for '{start}' - '{end}'");
            var audits = context.OrdersAuditsReadOnly
                .Where(o => o.RecordDate >= start)
                .Where(o => o.RecordDate <= end)
                .OrderByDescending(o => o.RecordDate)
                .ToList()
                .ToCsv();
            var zip = compressor.Zip(audits, $"{yesterday:yyyyMMdd}.csv");

            Directory.CreateDirectory("./AuditZips"); // If the directory already exists, this method does nothing.
            File.WriteAllBytes($"./AuditZips/{yesterday:yyyyMMdd}.zip", zip);

            sw.Stop();
            var elapsed = sw.Elapsed;
            logger.LogInformation($"ZipJob took {elapsed.TotalSeconds} sec");
        }
    }
}

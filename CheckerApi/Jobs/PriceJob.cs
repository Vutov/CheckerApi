using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CheckerApi.Extensions;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class PriceJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var dataExtractor = serviceProvider.GetService<IDataExtractorService>();
            var logger = serviceProvider.GetService<ILogger<PriceJob>>();

            var url = config.GetValue<string>("Price:Url");
            var req = config.GetValue<string>("Price:Request");
            var pattern = config.GetValue<string>("Price:Regex");

            var result = dataExtractor.GetData(url, req, pattern);
            if (result.HasFailed())
            {
                logger.LogError(result.Messages.ToCommaSeparated());
                return;
            }

            var groups = result.Value?.ToList() ?? new List<string>();
            if (!groups.Any())
            {
                logger.LogWarning($"PriceJob empty result for '{url}{req}'");
                return;
            }

            var cache = serviceProvider.GetService<IMemoryCache>();
            var value = double.Parse(groups[1], CultureInfo.InvariantCulture);
            cache.Set(Constants.BtcBtgPriceKey, value);
        }
    }
}
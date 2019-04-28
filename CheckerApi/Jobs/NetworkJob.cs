using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CheckerApi.Extensions;
using CheckerApi.Models.Config;
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
    public class NetworkJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var dataExtractor = serviceProvider.GetService<IDataExtractorService>();
            var logger = serviceProvider.GetService<ILogger<NetworkJob>>();

            var url = config.GetValue<string>("Pool:Url");
            var req = config.GetValue<string>("Pool:Request");
            var pattern = config.GetValue<string>("Pool:Regex");

            var result = dataExtractor.GetData(url, req, pattern);
            if (result.HasFailed())
            {
                logger.LogError(result.Messages.ToCommaSeparated());
                return;
            }

            var groups = result.Value?.ToList() ?? new List<string>();
            if (!groups.Any())
            {
                logger.LogWarning($"PriceJob empty result for {url}{req}");
                return;
            }

            var cache = serviceProvider.GetService<IMemoryCache>();

            var configDenomination = config.GetValue<string>("Pool:Denomination");
            var denomination = (Denomination)Enum.Parse(typeof(Denomination), configDenomination, ignoreCase: true);
            var networkRate = double.Parse(groups[1], CultureInfo.InvariantCulture);
            var networkRateInMh = DenominationHelper.ToMSol(networkRate, denomination);
            cache.Set(Constants.HashRateKey, networkRateInMh);

            // Assume Difficulty is second group
            if (groups.Count > 2)
            {
                var difficulty = double.Parse(groups[2], CultureInfo.InvariantCulture);
                cache.Set(Constants.DifficultyKey, difficulty);
            }
        }
    }
}
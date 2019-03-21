using System;
using System.Globalization;
using System.Linq;
using CheckerApi.Models.Config;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var url = config.GetValue<string>("Pool:Url");
            var req = config.GetValue<string>("Pool:Request");
            var pattern = config.GetValue<string>("Pool:Regex");

            var result = dataExtractor.GetData(url, req, pattern).ToList();
            var networkRate = double.Parse(result[1], CultureInfo.InvariantCulture);
            var difficulty = double.Parse(result[2], CultureInfo.InvariantCulture);

            var cache = serviceProvider.GetService<IMemoryCache>();

            var denomination = (Denomination)Enum.Parse(typeof(Denomination), config.GetValue<string>("Pool:Denomination"), ignoreCase: true);
            var networkRateInMh = DenominationHelper.ToMSol(networkRate, denomination);
            cache.Set(Constants.HashRateKey, networkRateInMh);
            cache.Set(Constants.DifficultyKey, difficulty);
        }
    }
}
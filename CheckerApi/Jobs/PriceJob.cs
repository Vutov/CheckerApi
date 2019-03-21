using System;
using System.Globalization;
using System.Linq;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var url = config.GetValue<string>("Price:Url");
            var req = config.GetValue<string>("Price:Request");
            var pattern = config.GetValue<string>("Price:Regex");

            var result = dataExtractor.GetData(url, req, pattern).ToList();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var value = double.Parse(result[1], CultureInfo.InvariantCulture);
            cache.Set(Constants.BtcBtgPriceKey, value);
        }
    }
}
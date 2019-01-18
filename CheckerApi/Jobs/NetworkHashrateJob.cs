using System;
using System.Diagnostics;
using CheckerApi.Models.DTO;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using RestSharp;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class NetworkHashrateJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<NetworkHashrateJob>>();
            logger.LogInformation("Hashrate pull started");
            var sw = Stopwatch.StartNew();
            var config = serviceProvider.GetService<IConfiguration>();
            var url = config.GetValue<string>("Pool:Url");
            var req = config.GetValue<string>("Pool:Request");
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new RestClient(url);
                    var request = new RestRequest(req, Method.GET);
                    var response = client.Execute(request);
                    var result = JsonConvert.DeserializeObject<HashrateDTO>(response.Content);

                    var cache = serviceProvider.GetService<IMemoryCache>();
                    cache.Set(Constants.HashRateKey, result.Pool.NetworkStats.Rate);
                }
                catch (Exception ex)
                {
                    logger.LogCritical($"Hashrate error: {ex}");
                }
            }

            sw.Stop();
            var elapsed = sw.Elapsed;
            logger.LogInformation($"Hashrate pull took {elapsed.TotalSeconds} sec");
        }
    }
}

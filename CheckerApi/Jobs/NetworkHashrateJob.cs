using System;
using CheckerApi.Models.DTO;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var config = serviceProvider.GetService<IConfiguration>();
            var url = config.GetValue<string>("Pool:Url");
            var req = config.GetValue<string>("Pool:Request");
            var client = new RestClient(url);
            var request = new RestRequest(req, Method.GET);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<HashrateDTO>(response.Content);

            var cache = serviceProvider.GetService<IMemoryCache>();
            var networkRate = result.Pool.NetworkStats.Rate;
            if (networkRate > 0)
            {
                var networkRateInMh = networkRate / 1000000; // in Mh/s
                cache.Set(Constants.HashRateKey, networkRateInMh);
            }
        }
    }
}
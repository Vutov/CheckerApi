using System;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            var cache = serviceProvider.GetService<IMemoryCache>();
            cache.Set(Constants.HashRateKey, 12);
            ////var data = JsonConvert.DeserializeObject<ResultDTO>(response.Content);
            ////var orders = data.Result.Orders.Select(o => CreateDTO(o, location)).ToList();
        }
    }
}

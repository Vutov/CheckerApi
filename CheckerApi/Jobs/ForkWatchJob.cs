using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class ForkWatchJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var watchService = serviceProvider.GetService<IForkWatchService>();

            var rpcConfig = this.GetNodeRpcConfig(config);
            watchService.Execute(rpcConfig);
        }
    }
}
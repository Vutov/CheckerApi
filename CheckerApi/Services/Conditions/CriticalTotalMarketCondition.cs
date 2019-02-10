using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Services.Conditions
{
    [GlobalCondition(4)]
    public class CriticalTotalMarketCondition : TotalMarketCondition, IHeartbeat
    {
        public CriticalTotalMarketCondition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            TotalHashThreshold = 1;
            MessagePrefix = "Critical ";
        }

        public (string, string, string) Status(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var totalOrderHash = aliveOrders.Sum(o => o.AcceptedSpeed);

            var cache = ServiceProvider.GetService<IMemoryCache>();
            var hasRate = cache.TryGetValue<double>(Constants.HashRateKey, out var networkRateInMh);

            var info = $"HEARTBEAT: {MessagePrefix}Market Total Threshold ";
            if (hasRate == false)
            {
                return (info, " No network hashrate present", string.Empty);
            }

            var niceHashRateInMh = totalOrderHash * 1000;
            var percentage = (niceHashRateInMh / networkRateInMh) * 100;
            return (info, $"current exposure is {percentage:F2}% ", $"NiceHash rate: {niceHashRateInMh:F6} Mh/s, BTG Network Rate {networkRateInMh:F6} Mh/s");
        }
    }
}

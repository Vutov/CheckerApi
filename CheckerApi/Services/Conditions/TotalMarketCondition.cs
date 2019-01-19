using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Services.Conditions
{
    [GlobalCondition(5)]
    public class TotalMarketCondition : Condition
    {
        public TotalMarketCondition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected double? TotalHashThreshold { get; set; }
        protected string MessagePrefix { get; set; } = string.Empty;

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var threshold = TotalHashThreshold ?? config.TotalHashThreshold;
            var foundOrders = new List<AlertDTO>();
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var totalOrderHash = aliveOrders.Sum(o => o.AcceptedSpeed);

            var cache = ServiceProvider.GetService<IMemoryCache>();
            var hasRate = cache.TryGetValue<double>(Constants.HashRateKey, out var networkRate);

            if (hasRate && totalOrderHash * 1000000 * threshold >= networkRate)
            {
                string condition = $"Condition: " +
                                  $"Active Orders Hash ({totalOrderHash * 1000000:F2}) above or equal to " +
                                  $"{threshold * 100:F2}% of " +
                                  $"Total Network Hash ({networkRate:F2}) ";
                string message = $"{MessagePrefix}Market Total Threshold ALERT - 'AT RISK'. ";

                foundOrders.Add(new AlertDTO()
                {
                    BidEntry = new BidEntry() { NiceHashId = "0", NiceHashDataCenter = 0 },
                    Condition = condition,
                    Message = message
                });
            }

            return foundOrders;
        }
    }
}

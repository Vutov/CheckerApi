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

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config, IEnumerable<PoolHashrate> poolData)
        {
            var threshold = TotalHashThreshold ?? config.TotalHashThreshold;
            var foundOrders = new List<AlertDTO>();
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var totalOrderHash = aliveOrders.Sum(o => o.AcceptedSpeed);
            var niceHashRateInMh = totalOrderHash * 1000; // in Mh/s

            var hasRate = Cache.TryGetValue<double>(Constants.HashRateKey, out var networkRateInMh);

            if (hasRate && niceHashRateInMh * threshold >= networkRateInMh)
            {
                string condition = $"Condition: " +
                                   $"Active Orders Hash ({niceHashRateInMh:F2} Mh/s) above or equal to " +
                                   $"{threshold * 100:F2}% (actual {niceHashRateInMh / networkRateInMh * 100:F2}%) of " +
                                   $"Total Network Hash ({networkRateInMh:F2}) Mh/s " +
                                   $"{this.CreateIsProfitableMessage(aliveOrders.Average(o => o.Price), "Average Price of ")} " +
                                   $"{this.AnalyzePools(poolData, niceHashRateInMh)}";
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

        private string AnalyzePools(IEnumerable<PoolHashrate> poolData, double networkSpike)
        {
            var data = poolData
                .Where(d => d.EntryDate > DateTime.UtcNow.AddMinutes(-15)) // take last 15 min
                .GroupBy(h => h.Name)
                .ToDictionary(
                    k => k.Key,
                    this.GetDelta
                );

            var sum = data.Sum(d => d.Value);
            var message = string.Empty;
            var displacement = sum / networkSpike;
            if (displacement > 0.3d)
            {
                message = $"Pool Analysis - {displacement * 100:F4}% located in tracked pools; Top 3 pools ";
                data.OrderByDescending(d => d.Value).Take(3).ToList().ForEach(d => message += $"'{d.Key}' : {d.Value:F6} Mh/s; ");
            }

            return message;
        }

        private double GetDelta(IGrouping<string, PoolHashrate> poolHashrates)
        {
            var sorted = poolHashrates.OrderBy(v => v.EntryDate).Select(v => v.Value).ToList();
            var min = sorted.First();
            var max = sorted.Last();

            var delta = max - min;
            if (delta > 0)
            {
                return delta;
            }

            return 0;
        }
    }
}

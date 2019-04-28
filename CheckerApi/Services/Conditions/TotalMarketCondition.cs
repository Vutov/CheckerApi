using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckerApi.Services.Conditions
{
    [GlobalCondition(5)]
    public class TotalMarketCondition : Condition
    {
        private static readonly int attackResetMin = 10;
        private static DateTime _attackStart;
        private static DateTime? _lastCheck;

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
            var averagePrice = aliveOrders.Where(o => o.AcceptedSpeed > 0).Average(o => o.Price);
            var niceHashRateInMh = totalOrderHash * 1000; // in Mh/s

            var hasRate = Cache.TryGetValue<double>(Constants.HashRateKey, out var networkRateInMh);

            // To cut back on alert spam report if:
            // a.Power > 100 % and NOT profitable
            // or
            // b.Power > 150 % and likely FOR PROFIT
            var modifier = 1.5;
            if (IsOverpaying(averagePrice))
            {
                modifier = 1;
            }

            if (hasRate && niceHashRateInMh * threshold >= networkRateInMh * modifier)
            {
                _attackStart = GetNewAttackStart();

                string condition = $"Condition: " +
                                   $"Active Orders Hash ({niceHashRateInMh:F2} Mh/s) above or equal to " +
                                   $"{threshold * 100:F2}% (actual {niceHashRateInMh / networkRateInMh * 100:F2}%) of " +
                                   $"Total Network Hash ({networkRateInMh:F2}) Mh/s " +
                                   $"{this.CreateIsProfitableMessage(averagePrice, "Average Price of ")} " +
                                   $"{this.AnalyzePools(poolData, niceHashRateInMh)}" +
                                   $"{this.BlockInfo()}";
                string message = $"{MessagePrefix}Market Total Threshold ALERT - 'AT RISK'. {CreateShortIsProfitableMessage(averagePrice)} ";

                foundOrders.Add(new AlertDTO()
                {
                    BidEntry = new BidEntry()
                    {
                        RecordDate = DateTime.UtcNow,
                        Algo = orders.FirstOrDefault()?.Algo,
                        Price = averagePrice,
                        Alive = true,
                        NiceHashId = "Aggregation",
                        NiceHashDataCenter = 0,
                        LimitSpeed = aliveOrders.Sum(o => o.LimitSpeed),
                        AcceptedSpeed = aliveOrders.Sum(o => o.AcceptedSpeed) * 1000, // in Mh/s
                    },
                    Condition = condition,
                    Message = message
                });
            }

            return foundOrders;
        }

        private DateTime GetNewAttackStart()
        {
            if (!_lastCheck.HasValue || _lastCheck.Value.AddMinutes(attackResetMin) < DateTime.UtcNow)
            {
                _lastCheck = DateTime.UtcNow;
                return DateTime.UtcNow;
            }

            _lastCheck = DateTime.UtcNow;
            return _attackStart;
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

        private string BlockInfo()
        {
            if (!Cache.TryGetValue<BlocksList>(Constants.BlocksInfoKey, out var blocksInfo))
            {
                return "No blocks info available";
            }

            var blocksSinceAttack = blocksInfo.GetSince(_attackStart).ToList();
            var averageTime = "---";
            var blocksDetails = "---";
            if (blocksSinceAttack.Any())
            {
                var blocksWithTimeSince = blocksSinceAttack.Where(b => b.TimeSinceLast.HasValue).Select(b => b.TimeSinceLast.Value.TotalMinutes).ToList();
                if (blocksWithTimeSince.Any())
                {
                    averageTime = $"{blocksWithTimeSince.Average():F1}";
                }

                blocksDetails = string.Join(", ", blocksSinceAttack.Select(b =>
                {
                    var minutes = "---";
                    if (b.TimeSinceLast != null)
                    {
                        minutes = $"{b.TimeSinceLast.Value.TotalMinutes:F1}";
                    }

                    return $"{b.Height} - {minutes} m";
                }));
            }

            return $" Found blocks since attack started at {_attackStart:T} UTC - {blocksSinceAttack.Count()} blocks;" +
                   $" Average block time: {averageTime} minutes;" +
                   $" Details: {blocksDetails}; ";
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using CheckerApi.Extensions;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Conditions
{
    public class PercentThresholdCondition: Condition
    {
        public static Queue<string> PercentageTrack = new Queue<string>();

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var foundOrders = new List<AlertDTO>();
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var threshold = aliveOrders.Sum(o => o.AcceptedSpeed) * config.AcceptedPercentThreshold;
            var orderedOrders = aliveOrders.OrderByDescending(o => o.Price);
            var currentAcceptedSpeed = 0d;

            BidEntry benchmarkOrder = null;
            foreach (var order in orderedOrders)
            {
                if (currentAcceptedSpeed + order.AcceptedSpeed >= threshold)
                {
                    benchmarkOrder = order;
                    break;
                }

                currentAcceptedSpeed += order.AcceptedSpeed;
            }

            if (benchmarkOrder == null)
            {
                return foundOrders;
            }
            
            foreach (var order in aliveOrders)
            {
                if (order.Price >= benchmarkOrder.Price &&
                   (order.LimitSpeed == 0 || order.LimitSpeed >= config.LimitSpeed) &&
                    order.AcceptedSpeed >= config.MinimalAcceptedSpeed
                ) // todo add min accepted speed test
                {
                    var sig = CreateSignSignature(order);
                    string conditon;
                    string message;

                    if (!PercentageTrack.Contains(sig))
                    {
                        PercentageTrack.ConditionEnqueue(sig);

                        conditon = $"Condition: " +
                                   $"Order Alive ({order.Alive}) AND " +
                                   $"Order Price ({order.Price}) above '{benchmarkOrder.Price}' benchmark Order Price (ID: {benchmarkOrder.NiceHashId}) AND " +
                                   $"(Order Speed Limit ({order.LimitSpeed}) = 0 OR Order Speed Limit ({order.LimitSpeed}) >= '{config.LimitSpeed}') AND " +
                                   $"Order Accepted Speed ({order.AcceptedSpeed}) >= {config.MinimalAcceptedSpeed}. ";
                        message = $"SUSPICIOUS BID Percentage ALERT - an attack may be about to begin. {CreateMessage(order)}. ";
                    }
                    else
                    {
                        conditon = string.Empty;
                        message = $"SUSPICIOUS BID Progress - {CreateMessageForProgress(order)}. ";
                    }

                    foundOrders.Add(new AlertDTO()
                    {
                        BidEntry = order,
                        Condition = conditon,
                        Message = message
                    });
                }
            }

            return foundOrders;
        }
    }
}

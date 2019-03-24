using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Extensions;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Conditions
{
    [Condition(20)]
    public class SignOfAttackCondition : Condition
    {
        private static readonly Queue<string> BidsTrack = new Queue<string>();

        public SignOfAttackCondition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config, IEnumerable<PoolHashrate> poolData)
        {
            var foundOrders = new List<AlertDTO>();
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var highestOrder = aliveOrders.OrderByDescending(o => o.Price).FirstOrDefault();
            if (highestOrder == null)
            {
                return foundOrders;
            }

            foreach (var order in aliveOrders)
            {
                if (order.Price + config.PriceThreshold >= highestOrder.Price &&
                    (order.LimitSpeed == 0 || order.LimitSpeed >= config.LimitSpeed) &&
                    order.AcceptedSpeed >= config.MinimalAcceptedSpeed
                )
                {
                    var sig = CreateSignSignature(order);
                    string condition;
                    string message;

                    if (!BidsTrack.Contains(sig))
                    {
                        BidsTrack.ConditionEnqueue(sig);

                        condition = $"Condition: " +
                                    $"Order Alive ({order.Alive}) AND " +
                                    $"Order Price ({order.Price}) within '{config.PriceThreshold}' of top Order Price ({highestOrder.Price}, ID: {highestOrder.NiceHashId}) AND " +
                                    $"(Order Speed Limit ({order.LimitSpeed}) = 0 OR Order Speed Limit ({order.LimitSpeed}) >= '{config.LimitSpeed}') AND" +
                                    $"Order Accepted Speed ({order.AcceptedSpeed}) >= {config.MinimalAcceptedSpeed}. " +
                                    $"{this.CreateIsProfitableMessage(order.Price)} ";
                        message = $"SUSPICIOUS BID ALERT - an attack may be about to begin. {CreateMessage(order)}. ";
                    }
                    else
                    {
                        condition = string.Empty;
                        message = $"SUSPICIOUS BID Progress - {CreateMessageForProgress(order)} .";
                    }

                    foundOrders.Add(new AlertDTO()
                    {
                        BidEntry = order,
                        Condition = condition,
                        Message = message
                    });
                }
            }

            return foundOrders;
        }
    }
}

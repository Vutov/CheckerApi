using System.Collections.Generic;
using System.Linq;
using CheckerApi.Data.DTO;
using CheckerApi.Data.Entities;
using CheckerApi.Extensions;

namespace CheckerApi.Services.Conditions
{
    public class SignOfAttackCondition : Condition
    {
        public static Queue<string> BidsTrack = new Queue<string>();

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config)
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
                ) // todo add min accepted speed test
                {
                    var sig = CreateSignSignature(order);
                    string conditon;
                    string message;

                    if (!BidsTrack.Contains(sig))
                    {
                       BidsTrack.ConditionEnqueue(sig);

                        conditon = $"Condition: " +
                                   $"Order Alive ({order.Alive}) AND " +
                                   $"Order Price ({order.Price}) within '{config.PriceThreshold}' of top Order Price ({highestOrder.Price}, ID: {highestOrder.NiceHashId}) AND " +
                                   $"(Order Speed Limit ({order.LimitSpeed}) = 0 OR Order Speed Limit ({order.LimitSpeed}) >= '{config.LimitSpeed}') AND" +
                                   $"Order Accepted Speed ({order.AcceptedSpeed}) >= {config.MinimalAcceptedSpeed}. ";
                        message = $"SUSPICIOUS BID ALERT - an attack may be about to begin. {CreateMessage(order)}. ";
                    }
                    else
                    {
                        conditon = string.Empty;
                        message = $"SUSPICIOUS BID Progress - {CreateMessageForProgress(order)} .";
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

using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Conditions
{
    [Condition(5)]
    public class TotalMarketCondition : Condition
    {
        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var foundOrders = new List<AlertDTO>();
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var totalHash = aliveOrders.Sum(o => o.AcceptedSpeed);
            //// TODO > 80% - add 80% to config
            ////if (order.Price >= benchmarkOrder.Price &&
            ////   (order.LimitSpeed == 0 || order.LimitSpeed >= config.LimitSpeed) &&
            ////    order.AcceptedSpeed >= config.MinimalAcceptedSpeed
            ////)
            ////{
            ////    string conditon = $"Condition: " +
            ////                   $"Order Alive ({order.Alive}) AND " +
            ////                   $"Order Price ({order.Price}) above '{benchmarkOrder.Price}' benchmark Order Price (ID: {benchmarkOrder.NiceHashId}) AND " +
            ////                   $"(Order Speed Limit ({order.LimitSpeed}) = 0 OR Order Speed Limit ({order.LimitSpeed}) >= '{config.LimitSpeed}') AND " +
            ////                   $"Order Accepted Speed ({order.AcceptedSpeed}) >= {config.MinimalAcceptedSpeed}. ";
            ////    string message = $"SUSPICIOUS BID Percentage ALERT - an attack may be about to begin. {CreateMessage(order)}. ";
            ////
            ////    foundOrders.Add(new AlertDTO()
            ////    {
            ////        // TODO? BidEntry = order,
            ////        Condition = conditon,
            ////        Message = message
            ////    });
            ////}

            return foundOrders;
        }
    }
}

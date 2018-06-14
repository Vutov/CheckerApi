using System.Collections.Generic;
using CheckerApi.Extensions;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using Newtonsoft.Json;

namespace CheckerApi.Services.Conditions
{
    [Condition(10)]
    public class AcceptedSpeedCondition: Condition
    {
        private static readonly Queue<string> DataHashes = new Queue<string>();

        public override IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var foundOrders = new List<AlertDTO>();
            foreach (var order in orders)
            {
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = Sha256(ordStr);

                if (!DataHashes.Contains(hash) &&
                    order.Alive &&
                    order.AcceptedSpeed >= config.AcceptedSpeed
                )
                {
                    DataHashes.ConditionEnqueue(hash);

                    foundOrders.Add(new AlertDTO()
                    {
                        BidEntry = order,
                        Condition = $"Condition: " +
                                    $"Order Alive ({order.Alive}) AND " +
                                    $"Order Accepted Speed ({order.AcceptedSpeed}) >= '{config.AcceptedSpeed}'. ",
                        Message = $"LARGE ORDER ALERT - possible attack in progress. {CreateMessage(order)}. "
                    });
                }
            }

            return foundOrders;
        }
    }
}

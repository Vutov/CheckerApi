using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CheckerApi.Data.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckerApi.Services
{
    public class ConditionComplier : IConditionComplier
    {
        private readonly Dictionary<int, string> _locationDict = new Dictionary<int, string>()
        {
            {0, "Europe"},
            {1, "USA"}
        };

        public static HashSet<string> DataHashes = new HashSet<string>();
        private readonly ILogger<ConditionComplier> _logger;

        public ConditionComplier(ILogger<ConditionComplier> logger)
        {
            _logger = logger;
        }

        public (IEnumerable<BidEntry> bids, string condition, string message) AcceptedSpeedCondition(IEnumerable<BidEntry> orders, int location, ApiConfiguration config)
        {
            var foundOrders = new List<BidEntry>();
            foreach (var order in orders)
            {
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = this.Sha256(ordStr);

                if (!DataHashes.Contains(hash) &&
                    order.Alive &&
                    order.AcceptedSpeed >= config.AcceptedSpeed
                )
                {
                    DataHashes.Add(hash);
                    foundOrders.Add(order);
                    _logger.LogInformation(ordStr);
                }
            }

            var bestOrder = foundOrders.OrderByDescending(o => o.AcceptedSpeed).FirstOrDefault();
            if (bestOrder != null)
            {
                return (foundOrders,
                        $"Order Alive ({bestOrder.Alive}) AND Order Accepted Speed ({bestOrder.AcceptedSpeed}) >= '{config.AcceptedSpeed}'",
                        $"LARGE ORDER ALERT - possible attack in progress. {this.CreateMessage(bestOrder)}"
                    );
            }

            return (foundOrders, string.Empty, string.Empty);
        }

        public (IEnumerable<BidEntry> bids, string condition, string message) SignOfAttack(IEnumerable<BidEntry> orders, int location, ApiConfiguration config)
        {
            var foundOrders = new List<BidEntry>();
            var top2 = orders.Where(o => o.Alive).OrderByDescending(o => o.Price).Take(2).ToList();
            if (top2.Count < 2)
            {
                return (foundOrders, string.Empty, string.Empty);
            }

            var ordStr = JsonConvert.SerializeObject(top2[0]);
            var hash = Sha256(ordStr);
            if (!DataHashes.Contains(hash) &&
                top2[0].Price - config.PriceThreshold >= top2[1].Price &&
               (top2[0].LimitSpeed == 0 || top2[0].LimitSpeed >= config.LimitSpeed)
            )
            {
                DataHashes.Add(hash);
                foundOrders.Add(top2[0]);
                _logger.LogInformation(ordStr);
            }

            if (foundOrders.Any())
            {
                return (foundOrders,
                        $"Order Alive ({top2[0].Alive}) AND Order Price ({top2[0].Price}) - '{config.PriceThreshold}' >= Second Order Price ({top2[1].Price}, ID: {top2[1].NiceHashId}) AND Order Speed Limit ({top2[0].LimitSpeed}) = 0 OR Order Speed Limit ({top2[0].LimitSpeed}) >= '{config.LimitSpeed}'",
                        $"SUSPICIOUS BID ALERT - an attack may be about to begin. {this.CreateMessage(top2[0])}"
                    );
            }

            return (foundOrders, string.Empty, string.Empty);
        }

        private string CreateMessage(BidEntry order)
        {
            var speedLimit = order.LimitSpeed == 0 ? "NO" : order.LimitSpeed.ToString(CultureInfo.InvariantCulture);
            return $"{order.AcceptedSpeed * 1000} MSol DELIVERED AT {order.RecordDate:G} WITH {speedLimit} LIMIT, PAYING {order.Price} ON ORDER ID {order.NiceHashId} AT {_locationDict[order.NiceHashDataCenter]} SERVER";
        }

        private string Sha256(string randomString)
        {
            using (var crypt = new SHA256Managed())
            {
                var hash = new StringBuilder();
                byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
                foreach (byte theByte in crypto)
                {
                    hash.Append(theByte.ToString("x2"));
                }

                return hash.ToString();
            }
        }
    }
}

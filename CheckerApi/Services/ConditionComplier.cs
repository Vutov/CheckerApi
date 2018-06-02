using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CheckerApi.Data.DTO;
using CheckerApi.Data.Entities;
using CheckerApi.Services.Interfaces;
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

        public static Queue<string> DataHashes = new Queue<string>();
        public static Queue<string> BidsTrack = new Queue<string>();
        
        public IEnumerable<AlertDTO> AcceptedSpeedCondition(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var foundOrders = new List<AlertDTO>();
            foreach (var order in orders)
            {
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = this.Sha256(ordStr);

                if (!DataHashes.Contains(hash) &&
                    order.Alive &&
                    order.AcceptedSpeed >= config.AcceptedSpeed
                )
                {
                    HandleQueue(hash, DataHashes);

                    foundOrders.Add(new AlertDTO()
                    {
                        BidEntry = order,
                        Condition = $"Condition: Order Alive ({order.Alive}) AND Order Accepted Speed ({order.AcceptedSpeed}) >= '{config.AcceptedSpeed}'. ",
                        Message = $"LARGE ORDER ALERT - possible attack in progress. {this.CreateMessage(order)}. "
                    });
                }
            }

            return foundOrders;
        }
        
        public IEnumerable<AlertDTO> SignOfAttack(IEnumerable<BidEntry> orders, ApiConfiguration config)
        {
            var foundOrders = new List<AlertDTO>();
            var top2 = orders.Where(o => o.Alive).OrderByDescending(o => o.Price).Take(2).ToList();
            if (top2.Count < 2)
            {
                return foundOrders;
            }
            
            if (top2[0].Price - config.PriceThreshold >= top2[1].Price &&
               (top2[0].LimitSpeed == 0 || top2[0].LimitSpeed >= config.LimitSpeed)
            )
            {
                var sig = this.CreateSignSignature(top2[0]);
                string conditon = string.Empty;
                string message = string.Empty;
                var order = top2[0];

                if (!BidsTrack.Contains(sig))
                {
                    HandleQueue(sig, BidsTrack);

                    conditon = $"Condition: Order Alive ({top2[0].Alive}) AND Order Price ({top2[0].Price}) - '{config.PriceThreshold}' >= Second Order Price ({top2[1].Price}, ID: {top2[1].NiceHashId}) AND Order Speed Limit ({top2[0].LimitSpeed}) = 0 OR Order Speed Limit ({top2[0].LimitSpeed}) >= '{config.LimitSpeed}'. ";
                    message = $"SUSPICIOUS BID ALERT - an attack may be about to begin. {this.CreateMessage(top2[0])}. ";
                }
                else
                {
                    message = $"SUSPICIOUS BID Progress - {order.AcceptedSpeed * 1000} MSol DELIVERED, ID {order.NiceHashId} AT {_locationDict[order.NiceHashDataCenter]} SERVER. ";
                }

                foundOrders.Add(new AlertDTO()
                {
                    BidEntry = top2[0],
                    Condition = conditon,
                    Message = message
                });

            }
            
            return foundOrders;
        }

        private void HandleQueue(string key, Queue<string> storage)
        {
            storage.Enqueue(key);
            if (storage.Count > 100)
            {
                storage.Dequeue();
            }
        }

        private string CreateSignSignature(BidEntry entry)
        {
            return $"{entry.ID}{entry.NiceHashDataCenter}";
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

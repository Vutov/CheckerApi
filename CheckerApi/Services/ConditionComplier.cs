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
            var aliveOrders = orders.Where(o => o.Alive).ToList();
            var highestOrder = aliveOrders.OrderByDescending(o => o.Price).FirstOrDefault();
            if (highestOrder == null)
            {
                return foundOrders;
            }

            foreach (var order in aliveOrders)
            {
                if (order.Price + config.PriceThreshold >= highestOrder.Price &&
                    (order.LimitSpeed == 0 || order.LimitSpeed >= config.LimitSpeed)
                )
                {
                    var sig = this.CreateSignSignature(order);
                    string conditon = string.Empty;
                    string message = string.Empty;

                    if (!BidsTrack.Contains(sig))
                    {
                        HandleQueue(sig, BidsTrack);

                        conditon = $"Condition: Order Alive ({order.Alive}) AND Order Price ({order.Price}) withing '{config.PriceThreshold}' top Order Price ({highestOrder.Price}, ID: {highestOrder.NiceHashId}) AND Order Speed Limit ({order.LimitSpeed}) = 0 OR Order Speed Limit ({order.LimitSpeed}) >= '{config.LimitSpeed}'. ";
                        message = $"SUSPICIOUS BID ALERT - an attack may be about to begin. {this.CreateMessage(order)}. ";
                    }
                    else
                    {
                        message = $"SUSPICIOUS BID Progress - {order.AcceptedSpeed * 1000} MSol DELIVERED, ID {order.NiceHashId} AT {_locationDict[order.NiceHashDataCenter]} SERVER. ";
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
            return $"{entry.NiceHashId}{entry.NiceHashDataCenter}";
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

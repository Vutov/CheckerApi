using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Services.Conditions
{
    public abstract class Condition: ICondition
    {
        private readonly Dictionary<int, string> _locationDict = new Dictionary<int, string>()
        {
            { 0, "Europe" },
            { 1, "USA" },
        };

        protected Condition(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Cache = ServiceProvider.GetService<IMemoryCache>();
        }

        protected IServiceProvider ServiceProvider { get; set; }
        protected IMemoryCache Cache { get; set; }

        public abstract IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config, IEnumerable<PoolHashrate> poolData);

        protected string CreateSignSignature(BidEntry entry)
        {
            return $"{entry.NiceHashId}{entry.NiceHashDataCenter}";
        }

        protected string CreateMessage(BidEntry order)
        {
            var speedLimit = order.LimitSpeed == 0 ? "NO" : order.LimitSpeed.ToString(CultureInfo.InvariantCulture);
            return $"{order.AcceptedSpeed * 1000} MSol DELIVERED AT {order.RecordDate:G} WITH {speedLimit} LIMIT, PAYING {order.Price} ON ORDER ID {order.NiceHashId} AT {_locationDict[order.NiceHashDataCenter]} SERVER";
        }

        protected string CreateMessageForProgress(BidEntry order)
        {
            return $"{order.AcceptedSpeed * 1000} MSol DELIVERED, ID {order.NiceHashId} AT {_locationDict[order.NiceHashDataCenter]} SERVER";
        }

        protected string Sha256(string randomString)
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

        /// <summary>
        /// Formula : BtcToBtgPrice x 131835937.5 / Difficulty
        /// Whenever the price paid on NiceHash is bigger than that, it's NOT going to be a hashrush for mining profit.
        /// That's based on the following formula for revenue in BT *G* per day for 1,000,000 Sol, or for 1 MSol:
        /// ( 12.5 * 1000000 / (D * 2^13) ) * 60 * 60 * 24 .
        /// </summary>
        /// <param name="payingPrice">Paying Price for the bid.</param>
        /// <param name="priceSummary">Summary of the price, if needed.</param>
        /// <returns>Message.</returns>
        protected string CreateIsProfitableMessage(double payingPrice, string priceSummary = "")
        {
            var hasDifficulty = Cache.TryGetValue<double>(Constants.DifficultyKey, out var networkDifficulty);
            var hasPrice = Cache.TryGetValue<double>(Constants.BtcBtgPriceKey, out var price);
            if (hasDifficulty && hasPrice)
            {
                var threshold = price * 131835937.5d / networkDifficulty;
                var status = payingPrice > threshold ? "Order is overpaying" : "Order is most likely FOR PROFIT";
                return $"Price Analysis: {status}. Paying {priceSummary}{payingPrice:F6} BTC, Revenue threshold {threshold:F6} BTC, Netowork Difficulty {networkDifficulty:F6}";
            }

            return $"Unsuficiant data for Price Analysis. Missing difficulty '{!hasDifficulty}', missing price '{!hasPrice}'";
        }
    }
}
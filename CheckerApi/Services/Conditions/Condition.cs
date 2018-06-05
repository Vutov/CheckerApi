using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CheckerApi.Data.DTO;
using CheckerApi.Data.Entities;
using CheckerApi.Services.Interfaces;

namespace CheckerApi.Services.Conditions
{
    public abstract class Condition: ICondition
    {
        protected readonly Dictionary<int, string> LocationDict = new Dictionary<int, string>()
        {
            {0, "Europe"},
            {1, "USA"}
        };

        public abstract IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config);
        
        protected string CreateSignSignature(BidEntry entry)
        {
            return $"{entry.NiceHashId}{entry.NiceHashDataCenter}";
        }

        protected string CreateMessage(BidEntry order)
        {
            var speedLimit = order.LimitSpeed == 0 ? "NO" : order.LimitSpeed.ToString(CultureInfo.InvariantCulture);
            return $"{order.AcceptedSpeed * 1000} MSol DELIVERED AT {order.RecordDate:G} WITH {speedLimit} LIMIT, PAYING {order.Price} ON ORDER ID {order.NiceHashId} AT {LocationDict[order.NiceHashDataCenter]} SERVER";
        }

        protected string CreateMessageForProgress(BidEntry order)
        {
            return $"{order.AcceptedSpeed * 1000} MSol DELIVERED, ID {order.NiceHashId} AT {LocationDict[order.NiceHashDataCenter]} SERVER";
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
    }
}

using System.Collections.Generic;

namespace CheckerApi.Models.Config
{
    public class PoolConfig
    {
        public string Url { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<CoinConfig> Coins { get; set; }
    }
}

using Newtonsoft.Json;

namespace CheckerApi.Models.DTO
{
    public class HashrateDTO
    {
        [JsonProperty(PropertyName = "pool")]
        public PoolDTO Pool { get; set; }
    }

    public class PoolDTO
    {
        [JsonProperty(PropertyName = "networkStats")]
        public NetworkStatsDTO NetworkStats { get; set; }
    }

    public class NetworkStatsDTO
    {
        [JsonProperty(PropertyName = "networkHashrate")]
        public double Rate { get; set; }
    }
}

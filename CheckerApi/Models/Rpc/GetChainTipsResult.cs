using Newtonsoft.Json;

namespace CheckerApi.Models.Rpc
{
    public class GetChainTipsResult
    {
        [JsonProperty("result")]
        public ChainTip[] Result { get; set; }
    }

    public class ChainTip
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("branchlen")]
        public int BranchLen { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}

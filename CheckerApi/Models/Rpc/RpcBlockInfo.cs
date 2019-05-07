using Newtonsoft.Json;

namespace CheckerApi.Models.Rpc
{
    public class RpcBlockInfo
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("previousblockhash")]
        public string PreviousBlockHash { get; set; }
    }

    public class RpcBlockResult
    {
        [JsonProperty("result")]
        public RpcBlockInfo Result { get; set; }
    }
}

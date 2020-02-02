using Newtonsoft.Json;

namespace CheckerApi.Models.Rpc
{
    public class RpcBlockInfoBase
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("previousblockhash")]
        public string PreviousBlockHash { get; set; }

        [JsonProperty("chainwork")]
        public string ChainWork { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }
    }

    public class RpcBlockInfo : RpcBlockInfoBase
    {
        [JsonProperty("tx")]
        public string[] Tx { get; set; }
    }

    public class RpcBlockInfoVerbose : RpcBlockInfoBase
    {
        [JsonProperty("tx")]
        public Transaction[] Tx { get; set; }
    }

    public class RpcBlockResult
    {
        [JsonProperty("result")]
        public RpcBlockInfo Result { get; set; }
    }
}

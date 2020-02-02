using Newtonsoft.Json;

namespace CheckerApi.Models.Rpc
{
    public class Transaction
    {
        [JsonProperty("txid")]
        public string TxId { get; set; }
    }
}

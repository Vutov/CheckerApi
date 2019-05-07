using Newtonsoft.Json;

namespace CheckerApi.Models.Rpc
{
    public class RpcResult
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}

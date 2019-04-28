using System.Net;

namespace CheckerApi.Models.Config
{
    public class RpcConfig
    {
        public string Url { get; set; }

        public int Port { get; set; }

        public NetworkCredential Credentials { get; set; }
    }
}

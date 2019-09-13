using System.Collections.Generic;
using Newtonsoft.Json;

namespace CheckerApi.Models.DTO
{
    public class ResultDTO
    {
        [JsonProperty(PropertyName = "list")]
        public IEnumerable<BidDTO> Orders { get; set; }
    }

    public class BidDTO
    {
        [JsonProperty(PropertyName = "limit")]
        public double LimitSpeed { get; set; }
        [JsonProperty(PropertyName = "alive")]
        public bool Alive { get; set; }
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "type")]
        public BidType Type { get; set; }
        [JsonProperty(PropertyName = "rigsCount")]
        public string Workers { get; set; }
        [JsonProperty(PropertyName = "algorithm")]
        public BidAlgo Algo { get; set; }
        [JsonProperty(PropertyName = "acceptedCurrentSpeed")]
        public double AcceptedSpeed { get; set; }
    }

    public class BidType
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class BidAlgo
    {
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
    }
}

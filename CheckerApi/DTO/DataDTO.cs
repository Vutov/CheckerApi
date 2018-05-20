using System.Collections.Generic;
using Newtonsoft.Json;

namespace CheckerApi.DTO
{
    class ResultDTO
    {
        [JsonProperty(PropertyName = "result")]
        public OrdersDTO Result { get; set; }
    }

    class OrdersDTO
    {
        [JsonProperty(PropertyName = "orders")]
        public IEnumerable<DataDTO> Orders { get; set; }
    }

    public class DataDTO
    {
        [JsonProperty(PropertyName = "limit_speed")]
        public double LimitSpeed { get; set; }
        [JsonProperty(PropertyName = "alive")]
        public bool Alive { get; set; }
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "workers")]
        public string Workers { get; set; }
        [JsonProperty(PropertyName = "algo")]
        public string Algo { get; set; }
        [JsonProperty(PropertyName = "accepted_speed")]
        public double AcceptedSpeed { get; set; }
    }
}

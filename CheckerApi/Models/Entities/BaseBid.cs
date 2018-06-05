using System;

namespace CheckerApi.Models.Entities
{
    public class BaseBid
    {
        public int ID { get; set; }
        public DateTime RecordDate { get; set; }
        public double LimitSpeed { get; set; }
        public bool Alive { get; set; }
        public double Price { get; set; }
        public string NiceHashId { get; set; }
        public string Type { get; set; }
        public string Workers { get; set; }
        public string Algo { get; set; }
        public double AcceptedSpeed { get; set; }
        public int NiceHashDataCenter { get; set; }
    }
}

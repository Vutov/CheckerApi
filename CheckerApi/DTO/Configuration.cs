using System;

namespace CheckerApi.DTO
{
    public class Configuration
    {
        public int ID { get; set; }
        public double AcceptedSpeed { get; set; }
        public double LimitSpeed { get; set; }
        public double PriceThreshold { get; set; }
        public DateTime LastNotification { get; set; }
    }
}

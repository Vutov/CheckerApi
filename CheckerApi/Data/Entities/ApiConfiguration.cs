using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckerApi.Data.Entities
{
    [Table("Configurations")]
    public class ApiConfiguration
    {
        public int ID { get; set; }
        public double AcceptedSpeed { get; set; }
        public double LimitSpeed { get; set; }
        public double PriceThreshold { get; set; }
        public DateTime LastNotification { get; set; }
    }
}

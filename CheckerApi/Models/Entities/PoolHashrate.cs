using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckerApi.Models.Entities
{
    [Table("PoolHashrate")]
    public class PoolHashrate
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime EntryDate { get; set; }
    }
}

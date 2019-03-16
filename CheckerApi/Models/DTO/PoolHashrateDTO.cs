using System;

namespace CheckerApi.Models.DTO
{
    public class PoolHashrateDTO
    {
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Denomination { get; set; }
    }
}

using CheckerApi.Models.Entities;

namespace CheckerApi.Models.DTO
{
    public class AlertDTO
    {
        public BidEntry BidEntry { get; set; }
        public string Condition { get; set; }
        public string Message { get; set; }
    }
}

using System;

namespace CheckerApi.Models.DTO
{
    public class BlockInfoDTO
    {
        public string Hash { get; set; }

        public int Height { get; set; }

        public DateTime Time { get; set; }

        public string PreviousBlockHash { get; set; }

        public TimeSpan? TimeSinceLast { get; set; }

        public override string ToString()
        {
            return $"{Height} - {Time:G}";
        }
    }
}

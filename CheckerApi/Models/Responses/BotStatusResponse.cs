using System.Collections.Generic;

namespace CheckerApi.Models.Responses
{
    public class BotStatusResponse
    {
        public string Status { get; set; }
        public int FoundOrders { get; set; }
        public int AuditCount { get; set; }
        public IEnumerable<string> Config { get; set; }
        public List<string> Conditions { get; set; }
        public double StoredNetworkRate { get; set; }
        public double StoredNetworkDifficulty { get; set; }
        public double StoredBtcBtgPrice { get; set; }
        public int StoredBlocks { get; set; }
        public int StoredBlocksTip { get; set; }
    }
}

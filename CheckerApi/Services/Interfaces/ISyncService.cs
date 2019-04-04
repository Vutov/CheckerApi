using System.Collections.Generic;
using CheckerApi.Models;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface ISyncService
    {
        Result RunSync();

        Result RunHeartbeat();

        // TODO Think how
        IEnumerable<List<BidEntry>> GetTotalOrders(bool enableAudit);
    }
}
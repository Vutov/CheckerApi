using System.Collections.Generic;
using CheckerApi.Models;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IAuditManager
    {
        Result CreateAudit(IEnumerable<BidEntry> bids);
    }
}

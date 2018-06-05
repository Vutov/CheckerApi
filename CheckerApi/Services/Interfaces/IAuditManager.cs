using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerApi.Models;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IAuditManager
    {
        Task<Result> CreateAudit(IEnumerable<BidEntry> bids);
    }
}

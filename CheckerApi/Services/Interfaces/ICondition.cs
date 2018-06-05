using System.Collections.Generic;
using CheckerApi.Data.DTO;
using CheckerApi.Data.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface ICondition
    {
        IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config);
    }
}

using System.Collections.Generic;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface ICondition
    {
        IEnumerable<AlertDTO> Compute(IEnumerable<BidEntry> orders, ApiConfiguration config);
    }
}

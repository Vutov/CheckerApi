using System.Collections.Generic;
using CheckerApi.Data.DTO;
using CheckerApi.Data.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IConditionComplier
    {
        IEnumerable<AlertDTO> AcceptedSpeedCondition(IEnumerable<BidEntry> orders, ApiConfiguration config);
        IEnumerable<AlertDTO> SignOfAttack(IEnumerable<BidEntry> orders, ApiConfiguration config);
    }
}

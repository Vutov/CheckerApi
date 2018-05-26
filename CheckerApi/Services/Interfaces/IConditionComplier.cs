using System.Collections.Generic;
using CheckerApi.Data.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IConditionComplier
    {
        (IEnumerable<BidEntry> bids, string condition, string message) AcceptedSpeedCondition(IEnumerable<BidEntry> orders, int location, ApiConfiguration config);
        (IEnumerable<BidEntry> bids, string condition, string message) SignOfAttack(IEnumerable<BidEntry> orders, int location, ApiConfiguration config);
    }
}

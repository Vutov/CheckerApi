using System.Collections.Generic;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IHeartbeat
    {
        (string, string, string) Status(IEnumerable<BidEntry> orders, ApiConfiguration config);
    }
}

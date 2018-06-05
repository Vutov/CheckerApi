using System.Collections.Generic;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IConditionComplier
    {
        IEnumerable<AlertDTO> Check(IEnumerable<BidEntry> orders, ApiConfiguration config, IEnumerable<ConditionSetting> settings);
    }
}

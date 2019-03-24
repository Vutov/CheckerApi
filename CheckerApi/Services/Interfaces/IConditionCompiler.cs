using System.Collections.Generic;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;

namespace CheckerApi.Services.Interfaces
{
    public interface IConditionCompiler
    {
        IEnumerable<AlertDTO> Check(IEnumerable<IEnumerable<BidEntry>> orders, ApiConfiguration config, IEnumerable<ConditionSetting> settings, IEnumerable<PoolHashrate> poolData);
        IEnumerable<(string, string, string)> GetHeartbeats(IEnumerable<IEnumerable<BidEntry>> orders, ApiConfiguration config, IEnumerable<ConditionSetting> settings);
    }
}

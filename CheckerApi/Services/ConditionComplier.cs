using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Context;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;

namespace CheckerApi.Services
{
    public class ConditionComplier: IConditionComplier
    {
        public IEnumerable<AlertDTO> Check(IEnumerable<BidEntry> orders, ApiConfiguration config, IEnumerable<ConditionSetting> settings)
        {
            var foundOrders = new List<AlertDTO>();
            var foundOrdersIDs = new HashSet<string>();

            // Conditions are in order of priority
            var conditions = Registry.Conditions.OrderBy(c => c.Key).ToList();
            foreach (var conditionEntry in conditions)
            {
                var setting = settings.FirstOrDefault(s => s.ConditionID == conditionEntry.Key);
                if (setting != null && setting.Enabled)
                {
                    ICondition condition = (ICondition) Activator.CreateInstance(conditionEntry.Value);
                    var data = condition.Compute(orders, config);
                    foreach (var alert in data)
                    {
                        // Avoid duplicate alerts
                        if (!foundOrdersIDs.Contains(alert.BidEntry.NiceHashId))
                        {
                            foundOrdersIDs.Add(alert.BidEntry.NiceHashId);
                            foundOrders.Add(alert);
                        }
                        
                    }
                }
            }
            
            return foundOrders;
        }
    }
}

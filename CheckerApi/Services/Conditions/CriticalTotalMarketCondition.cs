using System;

namespace CheckerApi.Services.Conditions
{
    [Condition(4)]
    public class CriticalTotalMarketCondition : TotalMarketCondition
    {
        public CriticalTotalMarketCondition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            TotalHashThreshold = 1;
            MessagePrefix = "Critical ";
        }
    }
}

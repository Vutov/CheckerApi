using System;
using System.Collections.Generic;
using CheckerApi.Services.Conditions;

namespace CheckerApi.Context
{
    public static class Registry
    {
        public static readonly SortedDictionary<int, Type> Conditions = new SortedDictionary<int, Type>()
        {
            {10, typeof(AcceptedSpeedCondition) },
            {20, typeof(SignOfAttackCondition) },
            {30, typeof(PercentThresholdCondition) },
        };
    }
}
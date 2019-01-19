using System;

namespace CheckerApi.Services.Conditions
{
    public class ConditionAttribute : Attribute
    {
        public ConditionAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; set; }
    }

    public class GlobalConditionAttribute : ConditionAttribute
    {
        public GlobalConditionAttribute(int priority) : base(priority)
        {
        }
    }
}

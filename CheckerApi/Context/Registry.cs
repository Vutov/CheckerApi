using System;
using System.Collections.Generic;
using System.Reflection;
using CheckerApi.Services.Conditions;
using CheckerApi.Services.Interfaces;
using ServiceStack;

namespace CheckerApi.Context
{
    public static class Registry
    {
        public static IEnumerable<Type> GetConditions()
        {
            var conditions = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(ConditionAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        conditions.Add(type);
                    }
                }
            }

            return conditions;
        }

        public static IEnumerable<Type> GetHeartbeats()
        {
            var conditions = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var isHeartbeat = type.HasInterface(typeof(IHeartbeat));
                    if (isHeartbeat)
                    {
                        conditions.Add(type);
                    }
                }
            }

            return conditions;
        }

        public static int GetPriority(Type condition)
        {
            return ((ConditionAttribute)condition.GetCustomAttributes(typeof(ConditionAttribute), false)[0]).Priority;
        }
    }
}
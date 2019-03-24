using System.Collections.Generic;

namespace CheckerApi.Extensions
{
    public static class ListExtensions
    {
        public static string ToCommaSeparated<T>(this IEnumerable<T> list)
        {
            return string.Join(",", list);
        }
    }
}

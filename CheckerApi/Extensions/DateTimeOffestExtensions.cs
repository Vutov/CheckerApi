using System;

namespace CheckerApi.Extensions
{
    public static class DateTimeOffestExtensions
    {
        public static DateTimeOffset StartOfDay(this DateTimeOffset date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset);
        }

        public static DateTimeOffset MiddleOfDay(this DateTimeOffset date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, 12, 0, 0, date.Offset);
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, date.Offset);
        }

        public static DateTimeOffset StartOfWeek(this DateTimeOffset date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).StartOfDay();
        }

        public static DateTimeOffset EndOfWeek(this DateTimeOffset date)
        {
            return date.StartOfWeek().AddDays(6).EndOfDay();
        }

        public static DateTimeOffset StartOfMonth(this DateTimeOffset date)
        {
            var startOfMonth = new DateTimeOffset(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second, date.Offset);
            return startOfMonth.StartOfDay();
        }

        public static DateTimeOffset EndOfMonth(this DateTimeOffset date)
        {
            var endOfMonth = new DateTimeOffset(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second, date.Offset);
            return endOfMonth.AddMonths(1).AddDays(-1).EndOfDay();
        }
    }
}
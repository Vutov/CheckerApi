using System;
using System.Collections.Generic;

namespace CheckerApi
{
    public partial class Storage
    {
        public static Dictionary<string, Data> Data = new Dictionary<string, Data>();
        public static List<ExtendedData> ExtendedData = new List<ExtendedData>();
        public static double AcceptedSpeed = 0.02;
        public static double LimitSpeed = 11;
        public static double PriceThreshold = 0.03;

        public static DateTime LastNotification = DateTime.UtcNow.AddMinutes(-2);
    }

    public class ExtendedData
    {
        public DateTime DateTime { get; set; }
        public Data Data { get; set; }
    }
}

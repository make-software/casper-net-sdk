using System;
using System.Globalization;

namespace NetCasperSDK.Utils
{
    public class DateUtils
    {
        public static DateTime FromString(string datetime)
        {
            return DateTime.Parse(datetime);
        }

        public static ulong ToEpochTime(string datetime)
        {
            TimeSpan t = DateTime.Parse(datetime, null, DateTimeStyles.AdjustToUniversal) - new DateTime(1970, 1, 1);
            return (ulong)t.TotalSeconds * 1000;
        }

        public static ulong ToEpochTime(DateTime datetime)
        {
            TimeSpan t = datetime - new DateTime(1970, 1, 1);
            return (ulong)t.TotalSeconds * 1000;
        }
        
        public static string ToISOString(ulong epochTimeInMillis)
        {
            return DateTime.UnixEpoch
                .AddMilliseconds(epochTimeInMillis)
                .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
        }
    }
}
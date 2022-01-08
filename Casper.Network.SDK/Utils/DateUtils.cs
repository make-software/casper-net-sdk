using System;
using System.Globalization;

namespace Casper.Network.SDK.Utils
{
    public class DateUtils
    {
        public static ulong ToEpochTime(string datetime)
        {
            TimeSpan t = DateTime.Parse(datetime, null, DateTimeStyles.AdjustToUniversal) 
                         - new DateTime(1970, 1, 1);
            
            return  (ulong)t.TotalMilliseconds;
        }

        public static ulong ToEpochTime(DateTime datetime)
        {
            TimeSpan t = datetime - new DateTime(1970, 1, 1);
            return (ulong)t.TotalMilliseconds;
        }
        
        public static string ToISOString(ulong epochTimeInMillis)
        {
            return DateTime.UnixEpoch
                .AddMilliseconds(epochTimeInMillis)
                .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
        }
    }
}
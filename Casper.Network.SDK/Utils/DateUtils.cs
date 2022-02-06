using System;
using System.Globalization;

namespace Casper.Network.SDK.Utils
{
    public class DateUtils
    {
        /// <summary>
        /// Converts an ISO formatted date string to epoch timestamp.
        /// </summary>
        public static ulong ToEpochTime(string datetime)
        {
            TimeSpan t = DateTime.Parse(datetime, null, DateTimeStyles.AdjustToUniversal) 
                         - new DateTime(1970, 1, 1);
            
            return  (ulong)t.TotalMilliseconds;
        }

        /// <summary>
        /// Converts a DateTime object to epoch timestamp.
        /// </summary>
        public static ulong ToEpochTime(DateTime datetime)
        {
            TimeSpan t = datetime - new DateTime(1970, 1, 1);
            return (ulong)t.TotalMilliseconds;
        }
        
        /// <summary>
        /// Converts an epoc timestamp (in milliseconds) to an ISO formatted date string.
        /// </summary>
        public static string ToISOString(ulong epochTimeInMillis)
        {
            return DateTime.UnixEpoch
                .AddMilliseconds(epochTimeInMillis)
                .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
        }
    }
}
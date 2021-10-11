using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;
using Humanizer.Localisation;

namespace NetCasperSDK.Converters
{
    public class HumanizeTTLConverter : JsonConverter<ulong>
    {
        public override ulong Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            ulong ttl = 0;

            string humanizedTtl = reader.GetString();
            if (humanizedTtl is null) return 0;
            
            string[] parts = humanizedTtl.Split(new char[] {' '});

            foreach (var xUnit in parts)
            {
                if (xUnit.Contains("ms"))
                    ttl += ulong.Parse(xUnit.Replace("ms", ""));
                else if(xUnit.Contains("s"))
                    ttl += ulong.Parse(xUnit.Replace("s", ""))*1000;
                else if(xUnit.Contains("m"))
                    ttl += ulong.Parse(xUnit.Replace("m", ""))*60*1000;
                else if(xUnit.Contains("h"))
                    ttl += ulong.Parse(xUnit.Replace("h", ""))*3600*1000;
                else if(xUnit.Contains("day"))
                    ttl += ulong.Parse(xUnit.Replace("day", ""))*24*3600*1000;
                else if(xUnit.Contains("d"))
                    ttl += ulong.Parse(xUnit.Replace("d", ""))*24*3600*1000;
                else
                    throw new Exception($"Unsupported TTL part {xUnit}");
            }

            return ttl;
        }

        public override void Write(
            Utf8JsonWriter writer,
            ulong ttlInMillis,
            JsonSerializerOptions options)
        {
            var ttlStr = TimeSpan.FromMilliseconds(ttlInMillis)
                .Humanize(5, CultureInfo.InvariantCulture,
                    TimeUnit.Day, TimeUnit.Millisecond, ",");
            ttlStr = ttlStr.Replace("milliseconds", "ms")
                .Replace("millisecond", "ms")
                .Replace("seconds", "s")
                .Replace("second", "s")
                .Replace("minutes", "m")
                .Replace("minute", "m")
                .Replace("hours", "h")
                .Replace("hour", "h")
                .Replace("days", "d")
                .Replace("day", "d")
                .Replace(" ", "")
                .Replace(",", " ");

            writer.WriteStringValue(ttlStr);
        }
    }
}
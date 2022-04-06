using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Converters
{
    public class HumanizeTTLConverter : JsonConverter<ulong>
    {
        public override ulong Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string humanizedTtl = reader.GetString();
            if (humanizedTtl is null) return 0;

            return DurationUtils.StringToMilliseconds(humanizedTtl);
        }

        public override void Write(
            Utf8JsonWriter writer,
            ulong ttlInMillis,
            JsonSerializerOptions options)
        {
            var ttlStr = DurationUtils.MillisecondsToString(ttlInMillis);

            writer.WriteStringValue(ttlStr);
        }
    }
}
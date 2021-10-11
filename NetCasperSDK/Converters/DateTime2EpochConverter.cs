using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Utils;

namespace NetCasperSDK.Converters
{
    public class DateTime2EpochConverter : JsonConverter<ulong>
    {
        public override ulong Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            DateUtils.ToEpochTime(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            ulong epochTimeInMillis,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(DateUtils.ToISOString(epochTimeInMillis));
    }
}
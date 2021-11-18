using System;
using System.Text.Json;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Converters
{
    public class BigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                System.Numerics.BigInteger.Parse(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            BigInteger value,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
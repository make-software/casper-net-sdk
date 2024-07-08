using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Converters
{
    public class HexBytesConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            Org.BouncyCastle.Utilities.Encoders.Hex.Decode(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            byte[] bytes,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(bytes));
    }
}
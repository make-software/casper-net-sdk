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
    
    public class HexBytesWithChecksumConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var hex = reader.GetString();
            var bytes = CEP57Checksum.Decode(hex, out var checksumResult);
            if (checksumResult == CEP57Checksum.InvalidChecksum)
                throw new JsonException("Wrong checksum in hexadecimal string.");

            return bytes;
        }

        public override void Write(
            Utf8JsonWriter writer,
            byte[] bytes,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(CEP57Checksum.Encode(bytes));
    }
}
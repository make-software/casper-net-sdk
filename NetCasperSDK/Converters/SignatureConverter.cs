using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.Converters
{
    public class SignatureConverter : JsonConverter<Signature>
    {
        public override Signature Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                Signature.FromHexString(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            Signature signature,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(signature.ToHexString());
    }
}
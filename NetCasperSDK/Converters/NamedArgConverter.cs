using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.Converters
{
    public class NamedArgConverter: JsonConverter<NamedArg>
    {
        public override NamedArg Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            reader.Read(); //JsonTokenType.StartArray
            var name = reader.GetString();
            reader.Read();
            CLValue clValue = null;
            if(reader.TokenType == JsonTokenType.StartObject)
                clValue = JsonSerializer.Deserialize<CLValue>(ref reader, options);
            reader.Read(); //JsonTokenType.EndArray
            return new NamedArg(name, clValue);
        }

        public override void Write(
            Utf8JsonWriter writer,
            NamedArg namedArg,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(namedArg.Name);
            JsonSerializer.Serialize(writer, namedArg.Value, options);
            writer.WriteEndArray();
        }
    }
}
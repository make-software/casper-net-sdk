using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Converters
{
    public class GenericListConverter<T, TConverter> : JsonConverter<List<T>>
    {
        public override List<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject &&
                reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Array/Object start token expected to deserialize a list of T");

            reader.Read(); // Start array

            List<T> list = new List<T>();

            var tConverter = Activator.CreateInstance(typeof(TConverter)) as JsonConverter;
            
            while (reader.TokenType == JsonTokenType.StartObject ||
                   reader.TokenType == JsonTokenType.StartArray ||
                   (tConverter is IDeserializeAsList && reader.TokenType == JsonTokenType.PropertyName))
            {
                var element = JsonSerializer.Deserialize<T>(ref reader, new JsonSerializerOptions()
                {
                    Converters = { tConverter }
                });
                reader.Read(); // end object/array
                
                list.Add(element);
            }

            return list;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<T> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            var tConverter = Activator.CreateInstance(typeof(TConverter)) as JsonConverter;
            var elOptions = new JsonSerializerOptions()
            {
                WriteIndented = options.WriteIndented,
                Converters = { tConverter }
            };
            foreach (var element in value)
            {
                JsonSerializer.Serialize(writer, element, elOptions);
            }
            writer.WriteEndArray();
        }
    }
}
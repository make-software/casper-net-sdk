using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.Converters
{
    public class ExecutableDeployItemConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Tuple<string,ExecutableDeployItem>);
        }
        
        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(ExecutableDeployItemInner),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;            
        }

        public class ExecutableDeployItemInner : JsonConverter<Tuple<string,ExecutableDeployItem>>
        {
            public ExecutableDeployItemInner(JsonSerializerOptions options)
            {
            }

            public override Tuple<string,ExecutableDeployItem> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var name = reader.GetString();
                reader.Read();

                ExecutableDeployItem item=null;
                if (name == "ModuleBytes")
                    item = JsonSerializer.Deserialize<ModuleBytesDeployItem>(ref reader, options);
                else if (name == "StoredContractByHash")
                    item = JsonSerializer.Deserialize<StoredContractByHashDeployItem>(ref reader, options);
                else if (name == "StoredContractByName")
                    item = JsonSerializer.Deserialize<StoredContractByNameDeployItem>(ref reader, options);
                else if (name == "StoredVersionContractByHash")
                    item = JsonSerializer.Deserialize<StoredVersionedContractByHashDeployItem>(ref reader, options);
                else if (name == "StoredVersionContractByName")
                    item = JsonSerializer.Deserialize<StoredVersionedContractByNameDeployItem>(ref reader, options);
                else if (name == "Transfer")
                    item = JsonSerializer.Deserialize<TransferDeployItem>(ref reader, options);
                else
                    throw new JsonException( $"Unexpected executable deploy item found: {name}");

                if(item==null)
                    throw new JsonException($"Could not deserialize an ExecutableDeployItem object with name {name}");
                
                return new Tuple<string, ExecutableDeployItem>(item.JsonPropertyName(), item);
            }

            public override void Write(
                Utf8JsonWriter writer,
                Tuple<string,ExecutableDeployItem> tuple,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(tuple.Item1);
                if (tuple.Item2 is ModuleBytesDeployItem mbItem)
                    JsonSerializer.Serialize(writer, mbItem, options);
                else if (tuple.Item2 is StoredContractByHashDeployItem schItem)
                    JsonSerializer.Serialize(writer, schItem, options);
                else if (tuple.Item2 is StoredContractByNameDeployItem scnItem)
                    JsonSerializer.Serialize(writer, scnItem, options);
                else if (tuple.Item2 is StoredVersionedContractByHashDeployItem svchItem)
                    JsonSerializer.Serialize(writer, svchItem, options);
                else if (tuple.Item2 is StoredVersionedContractByNameDeployItem svcnItem)
                    JsonSerializer.Serialize(writer, svcnItem, options);
                else if (tuple.Item2 is TransferDeployItem trItem)
                    JsonSerializer.Serialize(writer, trItem, options);
                else
                    throw new ArgumentOutOfRangeException(nameof(tuple), "Unexpected executable deploy item object");

                writer.WriteEndObject();
            }
        }
    }
}
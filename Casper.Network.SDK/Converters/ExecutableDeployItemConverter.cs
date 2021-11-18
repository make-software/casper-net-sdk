using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Converters
{
    public class ExecutableDeployItemConverter : JsonConverter<ExecutableDeployItem>
    {
        public override ExecutableDeployItem Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
                reader.Read();
            
            var name = reader.GetString();
            reader.Read();

            ExecutableDeployItem item = null;
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
                throw new JsonException($"Unexpected executable deploy item found: {name}");

            if (item == null)
                throw new JsonException($"Could not deserialize an ExecutableDeployItem object with name {name}");

            if (reader.TokenType == JsonTokenType.EndObject)
                reader.Read();
            return item;
        }

        public override void Write(
            Utf8JsonWriter writer,
            ExecutableDeployItem item,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(item.JsonPropertyName());
            if (item is ModuleBytesDeployItem mbItem)
                JsonSerializer.Serialize(writer, mbItem, options);
            else if (item is StoredContractByHashDeployItem schItem)
                JsonSerializer.Serialize(writer, schItem, options);
            else if (item is StoredContractByNameDeployItem scnItem)
                JsonSerializer.Serialize(writer, scnItem, options);
            else if (item is StoredVersionedContractByHashDeployItem svchItem)
                JsonSerializer.Serialize(writer, svchItem, options);
            else if (item is StoredVersionedContractByNameDeployItem svcnItem)
                JsonSerializer.Serialize(writer, svcnItem, options);
            else if (item is TransferDeployItem trItem)
                JsonSerializer.Serialize(writer, trItem, options);
            else
                throw new ArgumentOutOfRangeException(nameof(item), "Unexpected executable deploy item object");

            writer.WriteEndObject();
        }
    }
}
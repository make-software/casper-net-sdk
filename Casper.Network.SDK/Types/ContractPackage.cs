using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A disabled version of a contract.
    /// </summary>
    [JsonConverter(typeof(DisabledVersion.DisabledVersionConverter))]
    public class DisabledVersion
    {
        /// <summary>
        /// Contract version.
        /// </summary>
        [JsonPropertyName("contract_version")]
        public uint Version { get; init; }

        [JsonPropertyName("protocol_version_major")]
        public uint ProtocolVersionMajor { get; init; }

        public class DisabledVersionConverter : JsonConverter<DisabledVersion>
        {
            public override DisabledVersion Read
            (
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options
            )
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        doc.RootElement.TryGetProperty("protocol_version_major", out var protocolVersionMajor);
                        doc.RootElement.TryGetProperty("contract_version", out var contractVersion);
                        if (!protocolVersionMajor.ValueKind.Equals(JsonValueKind.Number) ||
                            !contractVersion.ValueKind.Equals(JsonValueKind.Number))
                            throw new JsonException("Cannot deserialize StoredValue. Number expected for protocol_version_major and contract_version.");
                        return new DisabledVersion()
                        {
                            ProtocolVersionMajor = protocolVersionMajor.GetUInt32(),
                            Version = contractVersion.GetUInt32()
                        };
                    }
                }

                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException("Cannot deserialize StoredValue. StartObject expected.");
                {
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        var versionsArray = JsonSerializer.Deserialize<List<uint>>(doc.RootElement.GetRawText());

                        if (versionsArray?.Count != 2)
                            throw new JsonException(
                                "Cannot deserialize StoredValue. Array of length 2 expected for disabled_version item.");

                        return new DisabledVersion()
                        {
                            ProtocolVersionMajor = versionsArray[0],
                            Version = versionsArray[1]
                        };
                    }
                }

            }


            public override void Write(
                Utf8JsonWriter writer,
                DisabledVersion value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for DisabledVersion not yet implemented");
            }
        }
    }

    /// <summary>
    /// Information related to an active version of a contract.
    /// </summary>
    public class ContractVersion
    {
        /// <summary>
        /// Hash for this version of the contract.
        /// </summary>
        [JsonPropertyName("contract_hash")]
        public string Hash { get; init; }

        /// <summary>
        /// Contract version.
        /// </summary>
        [JsonPropertyName("contract_version")]
        public uint Version { get; init; }

        [JsonPropertyName("protocol_version_major")]
        public uint ProtocolVersionMajor { get; init; }
    }

    /// <summary>
    /// Groups associate a set of URefs with a label.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Group label 
        /// </summary>
        [JsonPropertyName("group")]
        public string Label { get; init; }

        /// <summary>
        /// List of URefs associated with the group label.
        /// </summary>
        [JsonPropertyName("keys")]
        [JsonConverter(typeof(GenericListConverter<URef, GlobalStateKey.GlobalStateKeyConverter>))]
        public List<URef> Keys { get; init; }
    }

    /// <summary>
    /// A enum to determine the lock status of the contract package.
    /// </summary>
    public enum LockStatus
    {
        Locked,
        Unlocked,
    }

    /// <summary>
    /// Contract definition, metadata, and security container.
    /// </summary>
    public class ContractPackage
    {
        [JsonPropertyName("access_key")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef AccessKey { get; init; }

        /// <summary>
        /// List of disabled versions of a contract.
        /// </summary>
        [JsonPropertyName("disabled_versions")]
        public List<DisabledVersion> DisabledVersions { get; init; }

        /// <summary>
        /// Groups associate a set of URefs with a label. Entry points on a contract can be given
        /// a list of labels they accept and the runtime will check that a URef from at least one
        /// of the allowed groups is present in the caller’s context before execution.
        /// </summary>
        [JsonPropertyName("groups")]
        public List<Group> Groups { get; init; }

        /// <summary>
        /// List of active versions of a contract.
        /// </summary>
        [JsonPropertyName("versions")]
        public List<ContractVersion> Versions { get; init; }

        /// <summary>
        /// The current state of the contract package.
        /// </summary>
        [JsonPropertyName("lock_status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LockStatus LockStatus { get; init; }
    }
}
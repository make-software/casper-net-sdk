using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public enum SystemEntityType
    {
        /// <summary>
        /// Mint contract.
        /// </summary>
        Mint,

        /// <summary>
        /// Handle Payment contract.
        /// </summary>
        HandlePayment,

        /// <summary>
        /// Standard Payment contract.
        /// </summary>
        StandardPayment,

        /// <summary>
        /// Auction contract.
        /// </summary>
        Auction,
    }
    
    [JsonConverter(typeof(EntityKindConverter))]
    public class EntityKind
    {
        /// <summary>
        /// Package associated with a native contract implementation.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SystemEntityType? System { get; init; }

        /// <summary>
        /// Package associated with an Account hash.
        /// </summary>
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey Account { get; init; }

        /// <summary>
        /// Packages associated with Wasm stored on chain.
        /// </summary>
        public TransactionRuntime? SmartContract { get; init; }

        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class EntityKindConverter : JsonConverter<EntityKind>
        {
            public override EntityKind Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var property = reader.GetString();
                        reader.Read();
                        EntityKind entity = null;
                        switch (property)
                        {
                            case "Account":
                                entity = new EntityKind()
                                {
                                    Account = new AccountHashKey(reader.GetString()),
                                };
                                break;
                            case "SmartContract":
                                var tag = reader.GetString();
                                entity = new EntityKind()
                                {
                                    SmartContract = TransactionRuntime.FromString(tag),
                                };
                                break;
                            case "System":
                                entity = new EntityKind()
                                {
                                    System = EnumCompat.Parse<SystemEntityType>(reader.GetString()),
                                };
                                break;
                        }

                        reader.Read();
                        if (entity != null)
                            return entity;
                    }
                }

                throw new JsonException("Cannot deserialize EntityKind");
            }

            public override void Write(
                Utf8JsonWriter writer,
                EntityKind blockHeader,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for EntityKind not yet implemented");
            }
        }
    }

    /// <summary>
    /// A message topic.
    /// </summary>
    public class MessageTopic
    {
        /// <summary>
        /// The message topic name.
        /// </summary>
        [JsonPropertyName("topic_name")]
        public string TopicName { get; init; }

        /// <summary>
        /// The hash of the name of the message topic.
        /// </summary>
        [JsonPropertyName("topic_name_hash")]
        public string TopicNameHash { get; init; }
    }

    public class AddressableEntity
    {
        /// <summary>
        /// Casper Platform protocol version.
        /// </summary>
        [JsonPropertyName("protocol_version")]
        public string ProtocolVersion { get; init; }

        /// <summary>
        /// The type of Package.
        /// </summary>
        [JsonPropertyName("entity_kind")]
        public EntityKind EntityKind { get; init; }

        /// <summary>
        /// The hex-encoded address of the Package.
        /// </summary>
        [JsonPropertyName("package_hash")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public PackageKey Package { get; init; }

        /// <summary>
        /// The hash address of the contract wasm.
        /// </summary>
        [JsonPropertyName("byte_code_hash")]
        public string ByteCodeHash { get; init; }

        [JsonPropertyName("main_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef MainPurse { get; init; }

        /// <summary>
        /// Set of public keys allowed to provide signatures on deploys for the package
        /// </summary>
        [JsonPropertyName("associated_keys")]
        public List<AssociatedKey> AssociatedKeys { get; init; }

        /// <summary>
        /// Thresholds that have to be met when executing an action of a certain type.
        /// </summary>
        [JsonPropertyName("action_thresholds")]
        public ActionThresholds ActionThresholds { get; init; }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A wrapper class for different types of values stored in the global state.
    /// </summary>
    [JsonConverter(typeof(StoredValue.StoredValueConverter))]
    public class StoredValue
    {
        public Contract Contract { get; init; }

        public CLValue CLValue { get; init; }

        public Account Account { get; init; }

        public ContractWasm ContractWasm { get; init; }

        public ContractPackage ContractPackage { get; init; }

        public Transfer Transfer { get; init; }

        public DeployInfo DeployInfo { get; init; }

        public EraInfo EraInfo { get; init; }

        public Bid Bid { get; init; }
        
        public BidKind BidKind { get; init; }

        public List<WithdrawPurse> Withdraw { get; init; }

        public List<UnbondingPurse> Unbonding { get; init; }
        
        /// <summary>
        /// Stores an addressable entity.
        /// </summary>
        public AddressableEntity AddressableEntity { get; init; }
        
        /// <summary>
        /// Stores a package.
        /// </summary>
        public Package Package { get; init; }
        
        /// <summary>
        /// A record of byte code.
        /// </summary>
        public ByteCode ByteCode { get; init; }
        
        /// <summary>
        /// Stores a message topic.
        /// </summary>
        public MessageTopicSummary MessageTopic { get; init; }
        
        /// <summary>
        /// Stores a message checksum.
        /// </summary>
        public string Message { get; init; }
        
        /// <summary>
        /// Stores a NamedKey.
        /// </summary>
        public NamedKeyValue NamedKey { get; init; }
        
        /// <summary>
        /// Stores location, type and data for a gas pre-payment.
        /// </summary>
        public Prepayment Prepayment { get; init; }
        
        public EntryPoint EntryPoint { get; init; }
        
        /// <summary>
        /// Raw bytes. Similar to a [`crate::StoredValue::CLValue`] but does not incur overhead of a [`crate::CLValue`] and [`crate::CLType`].
        /// </summary>
        public string RawBytes { get; init; }
        
        public class StoredValueConverter : JsonConverter<StoredValue>
        {
            public override StoredValue Read
                (
                 ref Utf8JsonReader reader,
                 Type typeToConvert,
                 JsonSerializerOptions options
                )
            {
                try
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException("Cannot deserialize StoredValue. StartObject expected.");

                    StoredValue storedValue = new StoredValue();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Cannot deserialize StoredValue. PropertyName expected.");

                        var propertyName = reader.GetString();
                        
                        var propertyInfo = typeof(StoredValue).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (propertyInfo != null)
                        {
                            reader.Read();
                            var propertyValue = JsonSerializer.Deserialize(ref reader, propertyInfo.PropertyType, options);
                            propertyInfo.SetValue(storedValue, propertyValue);
                        }
                        else
                            throw new JsonException($"Unknown property: {propertyName}.");
                    }

                    return storedValue;
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Cannot deserialize StoredValue. Error during deserialization.", ex);
                }
            }


            public override void Write(
                Utf8JsonWriter writer,
                StoredValue value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for StoredValue not yet implemented");
            }
        }
    }
}
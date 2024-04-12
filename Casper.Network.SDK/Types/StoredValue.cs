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
    public class StoredValue
    {
        public Contract Contract { get; init; }

        public CLValue CLValue { get; init; }

        public Account Account { get; init; }

        public string ContractWasm { get; init; }

        public ContractPackage ContractPackage { get; init; }

        public Transfer Transfer { get; init; }

        public DeployInfo DeployInfo { get; init; }

        public EraInfo EraInfo { get; init; }

        public Bid Bid { get; init; }

        public List<UnbondingPurse> Withdraw { get; init; }

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
                        if (propertyInfo == null)
                            throw new JsonException($"Unknown property: {propertyName}.");

                        reader.Read();
                        var propertyValue = JsonSerializer.Deserialize(ref reader, propertyInfo.PropertyType, options);
                        propertyInfo.SetValue(storedValue, propertyValue);
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
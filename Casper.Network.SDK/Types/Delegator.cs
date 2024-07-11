using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    internal class PublicKeyAndDelegator
    {
        [JsonPropertyName("delegator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey DelegatorPublicKey { get; init; }
        
        [JsonPropertyName("delegator")]
        public Delegator Delegator { get; init; }
    }

    internal class DelegatorV1
    {
        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }

        [JsonPropertyName("delegatee")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey ValidatorPublicKey { get; init; }

        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey DelegatorPublicKey { get; init; }

        [JsonPropertyName("staked_amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger StakedAmount { get; init; }
    }
    
    /// <summary>
    /// A delegator associated with the given validator.
    /// </summary>
    public class Delegator
    {
        /// <summary>
        /// The purse that was used for delegating.
        /// </summary>
        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }

        /// <summary>
        /// Public key of the validator
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey ValidatorPublicKey { get; init; }

        /// <summary>
        /// Public Key of the delegator
        /// </summary>
        [JsonPropertyName("delegator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey DelegatorPublicKey { get; init; }

        /// <summary>
        /// Amount of Casper token (in motes) delegated
        /// </summary>
        [JsonPropertyName("staked_amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger StakedAmount { get; init; }
        
        [JsonPropertyName("vesting_schedule")]
        public VestingSchedule VestingSchedule { get; init; }
        
        public class PublicKeyAndDelegatorListConverter : JsonConverter<List<Delegator>>, IDeserializeAsList
        {
            public bool DeserializeAsList { get { return true; } }
            
            public override List<Delegator> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var delegators = new List<Delegator>();

                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();

                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        try
                        {
                            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                            {
                                if (document.RootElement.TryGetProperty("delegator_public_key",
                                        out var delegatorPublicKey))
                                {
                                    var pkAndDelegator =
                                        JsonSerializer.Deserialize<PublicKeyAndDelegator>(document.RootElement
                                            .GetRawText());
                                    delegators.Add(pkAndDelegator.Delegator);
                                }
                                else
                                {
                                    var pkAndDelegator =
                                        JsonSerializer.Deserialize<DelegatorV1>(document.RootElement.GetRawText());
                                    delegators.Add(new Delegator()
                                    {
                                        BondingPurse = pkAndDelegator.BondingPurse,
                                        StakedAmount = pkAndDelegator.StakedAmount,
                                        DelegatorPublicKey = pkAndDelegator.DelegatorPublicKey,
                                        ValidatorPublicKey = pkAndDelegator.ValidatorPublicKey,
                                    });
                                }
                            }

                            reader.Read(); // skip end object
                        }
                        catch (Exception e)
                        {
                            throw new JsonException("Cannot parse list of delegators. " + e.Message);
                        }
                    }

                    return delegators;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read(); // skip start object

                    while (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        reader.Read(); // skip public key
                        
                        var delegator =
                            JsonSerializer.Deserialize<Delegator>(ref reader, options);
                        reader.Read();
                        delegators.Add(delegator);
                    }

                    return delegators;
                }
                
                throw new JsonException("StartArray or StartObject token expected to deserialize a list of delegators.");

            }

            public override void Write(
                Utf8JsonWriter writer,
                List<Delegator> value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for Delegator not yet implemented");
            }
        }
    }
}
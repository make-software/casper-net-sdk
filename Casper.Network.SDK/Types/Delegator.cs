using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A delegator associated with the given validator.
    /// </summary>
    public class Delegator
    {
        /// <summary>
        /// The purse that was used for delegating.
        /// </summary>
        public URef BondingPurse { get; init; }

        /// <summary>
        /// Public key of the validator
        /// </summary>
        public PublicKey Delegatee { get; init; }

        /// <summary>
        /// Public Key of the delegator
        /// </summary>
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Amount of Casper token (in motes) delegated
        /// </summary>
        public BigInteger StakedAmount { get; init; }
        
        public VestingSchedule VestingSchedule { get; init; }
    
        public class DelegatorConverter : JsonConverter<Delegator>, IDeserializeAsList
        {
            public bool DeserializeAsList { get { return true; } }
            
            public override Delegator Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                // two possibilities called 'Delegator' and 'JsonDelegator' in the rpc schema
                // An array of delegators is returned in the GetAuctionState response
                // A dictionary of delegators is returned in the QueryGlobalState response for a Bid key
                // This methods parses both but returns a common Delegator object in both cases

                string delegatorPublicKey = null;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    delegatorPublicKey = reader.GetString();
                    reader.Read();
                }

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Could not deserialize Delegator. Start object token expected.");

                reader.Read(); //start object

                string public_key = null;
                string amount = null;
                string bonding_purse = null;
                string validator_public_key = null;
                VestingSchedule vesting_schedule = null;
                
                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString()?.ToLowerInvariant();
                    reader.Read();
                    
                    switch (property)
                    {
                        case "delegator_public_key":
                        case "public_key":
                            public_key = reader.GetString();
                            reader.Read();
                            break;
                        case "bonding_purse":
                            bonding_purse = reader.GetString();
                            reader.Read();
                            break;
                        case "staked_amount":
                            amount = reader.GetString();
                            reader.Read();
                            break;
                        case "validator_public_key":
                        case "delegatee":
                            validator_public_key = reader.GetString();
                            reader.Read();
                            break;
                        case "vesting_schedule":
                            if(reader.TokenType != JsonTokenType.Null)
                                vesting_schedule = JsonSerializer.Deserialize<VestingSchedule>(ref reader, options);
                            reader.Read();
                            break;
                    }
                }

                return new Delegator()
                {
                    PublicKey = PublicKey.FromHexString(public_key),
                    StakedAmount = BigInteger.Parse(amount),
                    BondingPurse = new URef(bonding_purse),
                    Delegatee = PublicKey.FromHexString(validator_public_key),
                    VestingSchedule = vesting_schedule
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                Delegator value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for Delegator not yet implemented");
            }
        }
    }
}
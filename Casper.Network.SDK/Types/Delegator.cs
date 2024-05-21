using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    public class PublicKeyAndDelegator
    {
        [JsonPropertyName("delegator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey DelegatorPublicKey { get; init; }
        
        [JsonPropertyName("delegator")]
        public Delegator Delegator { get; init; }
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
        
        // [JsonPropertyName("vesting_schedule")]
        // public VestingSchedule VestingSchedule { get; init; }
    }
}
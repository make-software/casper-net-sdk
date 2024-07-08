using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// An entry in the validator map.
    /// </summary>
    public class Bid
    {
        /// <summary>
        /// The purse that was used for bonding.
        /// </summary>
        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }

        /// <summary>
        /// The delegation rate.
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("delegation_rate")]
        public uint DelegationRate { get; init; }

        [JsonPropertyName("delegators")]
        [JsonConverter(typeof(Delegator.PublicKeyAndDelegatorListConverter))]
        public List<Delegator> Delegators { get; init; }
        
        /// <summary>
        /// `true` if validator has been "evicted"
        /// </summary>
        [JsonPropertyName("inactive")]
        public bool Inactive { get; init; }

        /// <summary>
        /// The amount of tokens staked by a validator (not including delegators).
        /// </summary>
        [JsonPropertyName("staked_amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger StakedAmount { get; init; }

        /// <summary>
        /// Validator public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Vesting schedule for a genesis validator. `None` if non-genesis validator.
        /// </summary>
        [JsonPropertyName("vesting_schedule")]
        public VestingSchedule VestingSchedule { get; init; }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using System.Numerics;

namespace Casper.Network.SDK.Types
{
    public class ValidatorBid
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
        /// Minimum allowed delegation amount in motes
        /// </summary>
        [JsonPropertyName("minimum_delegation_amount")]
        public ulong MinimumDelegationAmount { get; init; }
        
        /// <summary>
        /// Maximum allowed delegation amount in motes
        /// </summary>
        [JsonPropertyName("maximum_delegation_amount")]
        public ulong MaximumDelegationAmount { get; init; }
        
        /// <summary>
        /// Number of slots reserved for specific delegators
        /// </summary>
        [JsonPropertyName("reserved_slots")]
        public uint ReservedSlots { get; init; }
    }
}
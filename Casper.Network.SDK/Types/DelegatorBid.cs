using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Represents a party delegating their stake to a validator (or \"delegatee
    /// </summary>
    public class DelegatorBid
    {
        /// <summary>
        /// A Public key or Purse. Origin of the delegation. 
        /// </summary>
        [JsonPropertyName("delegator_kind")]
        public DelegatorKind DelegatorKind { get; init; }
        
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey ValidatorPublicKey { get; init; }
        
        /// <summary>
        /// Amount of Casper token (in motes) delegated
        /// </summary>
        [JsonPropertyName("staked_amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger StakedAmount { get; init; }
        
        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }
        
        [JsonPropertyName("vesting_schedule")]
        public VestingSchedule VestingSchedule { get; init; }
    }
}
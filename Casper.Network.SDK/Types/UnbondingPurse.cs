using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Information of a delegation withdrawal (legacy structure)
    /// </summary>
    public class WithdrawPurse
    {
        /// <summary>
        /// Unbonding Amount.
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }

        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }
        
        /// <summary>
        /// Era in which this unbonding request was created.
        /// </summary>
        [JsonPropertyName("era_of_creation")]
        public ulong EraOfCreation { get; init; }
        
        /// <summary>
        /// Unbonder public key.
        /// </summary>
        [JsonPropertyName("unbonder_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey UnbonderPublicKey { get; init; }
        
        /// <summary>
        /// Validators public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey ValidatorPublicKey { get; init; }
    }
    
    /// <summary>
    /// Information of an unbonding or delegation withdrawal
    /// </summary>
    public class UnbondingPurse : WithdrawPurse
    {
        
        /// <summary>
        /// The validator public key to re-delegate to.
        /// </summary>
        [JsonPropertyName("new_validator")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey NewValidator { get; init; }
    }
}

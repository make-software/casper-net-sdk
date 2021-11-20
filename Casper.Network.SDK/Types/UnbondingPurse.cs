using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    public class UnbondingPurse
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
        /// Unbonders public key.
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
}
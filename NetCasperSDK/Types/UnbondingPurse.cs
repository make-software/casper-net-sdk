using System.Numerics;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
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
        public string BondingPurse { get; init; }
        
        /// <summary>
        /// Era in which this unbonding request was created.
        /// </summary>
        [JsonPropertyName("era_of_creation")]
        public ulong EraOfCreation { get; init; }
        
        /// <summary>
        /// Unbonders public key.
        /// </summary>
        [JsonPropertyName("unbonder_public_key")]
        public string UnbonderPublicKey { get; init; }
        
        /// <summary>
        /// Validators public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        public string ValidatorPublicKey { get; init; }
    }
}
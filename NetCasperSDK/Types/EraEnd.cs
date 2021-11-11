using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// A validator reward 
    /// </summary>
    public class Reward
    {
        /// <summary>
        /// Reward amount
        /// </summary>
        [JsonPropertyName("amount")]
        public ulong Amount { get; init; }
        
        /// <summary>
        /// Validator public key
        /// </summary>
        [JsonPropertyName("validator")]
        public string PublicKey { get; init; }
    }
    
    /// <summary>
    /// Equivocation and reward information to be included in the terminal block.
    /// </summary>
    public class EraReport
    {
        /// <summary>
        /// List of public keys of the equivocators
        /// </summary>
        [JsonPropertyName("equivocators")]
        public List<string> Equivocators { get; init; }
        
        /// <summary>
        /// List of public keys of inactive validators
        /// </summary>
        [JsonPropertyName("inactive_validators")]
        public List<string> InactiveValidators { get; init; }
        
        /// <summary>
        /// List of validators with rewards
        /// </summary>
        [JsonPropertyName("rewards")]
        public List<Reward> Rewards { get; init; }
    }
    
    /// <summary>
    /// Era end report and list of validator weights for next era
    /// </summary>
    public class EraEnd
    {
        /// <summary>
        /// Era report
        /// </summary>
        [JsonPropertyName("era_report")]
        public EraReport EraReport { get; init; }
        
        /// <summary>
        /// A list of validator weights for the next era
        /// </summary>
        [JsonPropertyName("next_era_validator_weights")]
        [JsonConverter(typeof(GenericListConverter<ValidatorWeight, ValidatorWeight.ValidatorWeightConverter>))]
        public List<ValidatorWeight> NextEraValidatorWeights { get; init; }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    ///  validator's public key paired with a measure of the value of its contribution to consensus, as a fraction of the configured maximum block reward.
    /// </summary>
    public class Reward
    {
        /// <summary>
        /// The reward amount.
        /// </summary>
        [JsonPropertyName("amount")]
        public ulong Amount { get; init; }
        
        /// <summary>
        /// The validator's public key.
        /// </summary>
        [JsonPropertyName("validator")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }
    }
    
    /// <summary>
    /// Equivocation, reward and validator inactivity information.
    /// </summary>
    public class EraReport
    {
        /// <summary>
        /// The set of equivocators.
        /// </summary>
        [JsonPropertyName("equivocators")]
        [JsonConverter(typeof(GenericListConverter<PublicKey, PublicKey.PublicKeyConverter>))]
        public List<PublicKey> Equivocators { get; init; }
        
        /// <summary>
        /// Validators that haven't produced any unit during the era.
        /// </summary>
        [JsonPropertyName("inactive_validators")]
        [JsonConverter(typeof(GenericListConverter<PublicKey, PublicKey.PublicKeyConverter>))]
        public List<PublicKey> InactiveValidators { get; init; }
        
        /// <summary>
        /// Rewards for finalization of earlier blocks.
        /// </summary>
        [JsonPropertyName("rewards")]
        public List<Reward> Rewards { get; init; }
    }
    
    /// <summary>
    /// Information related to the end of an era, and validator weights for the following era.
    /// </summary>
    public class EraEndV1
    {
        /// <summary>
        /// Equivocation, reward and validator inactivity information.
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
    
        
    /// <summary>
    /// Information related to the end of an era, and validator weights for the following era.
    /// </summary>
    public class EraEndV2
    {
        /// <summary>
        /// The set of equivocators.
        /// </summary>
        [JsonPropertyName("equivocators")]
        [JsonConverter(typeof(GenericListConverter<PublicKey, PublicKey.PublicKeyConverter>))]
        public List<PublicKey> Equivocators { get; init; }
        
        /// <summary>
        /// Validators that haven't produced any unit during the era.
        /// </summary>
        [JsonPropertyName("inactive_validators")]
        [JsonConverter(typeof(GenericListConverter<PublicKey, PublicKey.PublicKeyConverter>))]
        public List<PublicKey> InactiveValidators { get; init; }
        
        /// <summary>
        /// A list of validator weights for the next era
        /// </summary>
        [JsonPropertyName("next_era_validator_weights")]
        [JsonConverter(typeof(GenericListConverter<ValidatorWeight, ValidatorWeight.ValidatorWeightConverter>))]
        public List<ValidatorWeight> NextEraValidatorWeights { get; init; }
        
        /// <summary>
        /// The rewards distributed to the validators.
        /// </summary>
        [JsonPropertyName("rewards")]
        public Dictionary<string, string> Rewards { get; init; }
        
        /// <summary>
        /// Next Era gas price
        /// </summary>
        [JsonPropertyName("next_era_gas_price")]
        public UInt16 NextEraGasPrice { get; init; }
    }
}
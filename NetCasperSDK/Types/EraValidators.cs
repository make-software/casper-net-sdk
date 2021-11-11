using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Validator and weights for an Era.
    /// </summary>
    public class EraValidators
    {
        /// <summary>
        /// The Era Id.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// List with each validator weight in the Era
        /// </summary>
        [JsonPropertyName("validator_weights")]
        [JsonConverter(typeof(GenericListConverter<ValidatorWeight, ValidatorWeight.ValidatorWeightConverter>))]
        public List<ValidatorWeight> ValidatorWeights { get; init; }
    }
}
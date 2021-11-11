using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// A change to a validator's status between two eras.
    /// </summary>
    public enum ValidatorChange
    {
        /// <summary>
        /// The validator got newly added to the validator set.
        /// </summary>
        Added,
        /// <summary>
        /// The validator was removed from the validator set.
        /// </summary>
        Removed,
        /// <summary>
        /// The validator was banned from this era.
        /// </summary>
        Banned,
        /// <summary>
        /// The validator was excluded from proposing new blocks in this era.
        /// </summary>
        CannotPropose,
        /// <summary>
        /// We saw the validator misbehave in this era.
        /// </summary>
        SeenAsFaulty
    }
    
    /// <summary>
    /// A single change to a validator's status in the given era.
    /// </summary>
    public class ValidatorStatusChange
    {
        /// <summary>
        /// The era in which the change occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The change in validator status.
        /// </summary>
        [JsonPropertyName("validator_change")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ValidatorChange ValidatorChange { get; init; }
    }

    /// <summary>
    /// The changes in a validator's status.
    /// </summary>
    public class ValidatorChanges
    {
        /// <summary>
        /// The public key of the validator.
        /// </summary>
        [JsonPropertyName("public_key")]
        public string PublicKey { get; init; }

        /// <summary>
        /// The set of changes to the validator's status.
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("status_changes")]
        public List<ValidatorStatusChange> ValidatorStatusChanges { get; init; }
    }
}
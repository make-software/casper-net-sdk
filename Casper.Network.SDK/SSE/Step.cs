using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The journal of execution transforms from an Era
    /// </summary>
    public class Step
    {
        /// <summary>
        /// The era in which the change occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The effect of executing the deploy.
        /// </summary>
        [JsonPropertyName("execution_effects")]
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Effects { get; init; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// The effect of executing a single deploy.
    /// </summary>
    public class ExecutionEffect
    {
        /// <summary>
        /// The resulting operations.
        /// </summary>
        [JsonPropertyName("operations")]
        public List<Operation> Operations { get; init; }

        /// <summary>
        /// The resulting transformations.
        /// </summary>
        [JsonPropertyName("transforms")]
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Transforms { get; init; }
    }
}
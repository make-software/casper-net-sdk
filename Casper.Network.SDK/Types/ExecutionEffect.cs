using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The journal of execution transforms from a single deploy.
    /// </summary>
    public class ExecutionEffect
    {
        /// <summary>
        /// The resulting operations.
        /// </summary>
        [JsonPropertyName("operations")]
        public List<Operation> Operations { get; init; }

        /// <summary>
        /// The journal of execution transforms.
        /// </summary>
        [JsonPropertyName("transforms")]
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Transforms { get; init; }
    }
}
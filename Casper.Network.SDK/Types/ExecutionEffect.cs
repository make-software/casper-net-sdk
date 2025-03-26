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
        [JsonConverter(typeof(GenericListConverter<TransformV1, TransformV1.TransformV1Converter>))]
        public List<TransformV1> Transforms { get; init; }
    }
}
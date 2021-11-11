using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Information relating to the given Deploy.
    /// </summary>
    public class DeployInfo
    {
        /// <summary>
        /// The relevant Deploy.
        /// </summary>
        [JsonPropertyName("deploy_hash")]
        public string DeployHash { get; init; }
        
        /// <summary>
        /// Account identifier of the creator of the Deploy.
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; init; }
        
        /// <summary>
        /// Gas cost of executing the Deploy.
        /// </summary>
        [JsonPropertyName("gas")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Gas { get; init; }
        
        /// <summary>
        /// Source purse used for payment of the Deploy.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; init; }
        
        /// <summary>
        /// Transfer addresses performed by the Deploy.
        /// </summary>
        [JsonPropertyName("transfers")]
        public List<string> Transfers { get; init; }
    }
}
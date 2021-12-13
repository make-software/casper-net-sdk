using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Information relating to the given Deploy.
    /// </summary>
    public class DeployInfo
    {
        /// <summary>
        /// The Deploy hash.
        /// </summary>
        [JsonPropertyName("deploy_hash")]
        public string DeployHash { get; init; }
        
        /// <summary>
        /// Account identifier of the creator of the Deploy.
        /// </summary>
        [JsonPropertyName("from")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey From { get; init; }
        
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
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef Source { get; init; }
        
        /// <summary>
        /// Transfer addresses performed by the Deploy.
        /// </summary>
        [JsonPropertyName("transfers")]
        [JsonConverter(typeof(GenericListConverter<TransferKey, GlobalStateKey.GlobalStateKeyConverter>))]
        public List<TransferKey> Transfers { get; init; }
    }
}
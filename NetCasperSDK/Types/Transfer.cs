using System.Numerics;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Represents a transfer from one purse to another
    /// </summary>
    public class Transfer
    {
        /// <summary>
        /// Transfer amount
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// Deploy that created the transfer
        /// </summary>
        [JsonPropertyName("deploy_hash")]
        public string DeployHash { get; init; }
        
        /// <summary>
        /// Account hash from which transfer was executed
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; init; }
        
        /// <summary>
        /// Gas
        /// </summary>
        [JsonPropertyName("gas")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Gas { get; init; }
        
        /// <summary>
        /// User-defined id
        /// </summary>
        [JsonPropertyName("id")]
        public ulong? Id { get; init; }
        
        /// <summary>
        /// Source purse
        /// </summary>
        [JsonPropertyName("source")]
        [JsonConverter(typeof(URef.URefConverter))]
        public URef Source { get; init; }
        
        /// <summary>
        /// Target purse
        /// </summary>
        [JsonPropertyName("target")]
        [JsonConverter(typeof(URef.URefConverter))]
        public URef Target { get; init; }
        
        /// <summary>
        /// Account to which funds are transferred
        /// </summary>
        [JsonPropertyName("to")]
        public string To { get; init; }
    }
}
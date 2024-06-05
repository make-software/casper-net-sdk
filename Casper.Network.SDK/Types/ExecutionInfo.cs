using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public class ExecutionInfo
    {
        /// <summary>
        /// The hash of the block in which the deploy was executed.
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The height of the block in which the deploy was executed.
        /// </summary>
        [JsonPropertyName("block_height")]
        public ulong BlockHeight { get; init; }
        
        /// <summary>
        /// The map of block hash to execution result.
        /// </summary>
        [JsonPropertyName("execution_result")]
        [JsonConverter(typeof(ExecutionResult.ExecutionResultConverter))]
        public ExecutionResult ExecutionResult { get; init; }
    }
}
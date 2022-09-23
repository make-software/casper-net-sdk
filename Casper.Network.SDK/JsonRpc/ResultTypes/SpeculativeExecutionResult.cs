using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "speculative_exec" RPC response.
    /// </summary>
    public class SpeculativeExecutionResult : RpcResult
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The result of executing the <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("execution_result")]
        [JsonConverter(typeof(ExecutionResult.ExecutionResultConverter))]
        public ExecutionResult ExecutionResult { get; init; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_deploy" RPC response.
    /// </summary>
    public class GetDeployResult : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("deploy")]
        public Deploy Deploy { get; init; }

        /// <summary>
        /// The map of block hash to execution result.
        /// </summary>
        [JsonPropertyName("execution_result")]
        public ExecutionResult ExecutionResult { get; init; }
        
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
    }
}

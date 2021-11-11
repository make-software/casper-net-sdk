using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "chain_get_block" RPC response.
    /// </summary>
    public class GetBlockResult : RpcResult
    {
        /// <summary>
        /// The block, if found.
        /// </summary>
        [JsonPropertyName("block")]
        public Block Block { get; init; }
    }
}
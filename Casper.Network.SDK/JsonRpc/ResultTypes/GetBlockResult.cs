using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "chain_get_block" RPC response.
    /// </summary>
    public class GetBlockResult : RpcResult
    {
        /// <summary>
        /// The block, if found.
        /// </summary>
        [JsonPropertyName("block_with_signatures")]
        public BlockWithSignatures BlockWithSignatures { get; init; }
    }
}
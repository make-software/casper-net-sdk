using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "chain_get_block_transfers" RPC response.
    /// </summary>
    public class GetBlockTransfersResult : RpcResult
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The block's transfers
        /// </summary>
        [JsonPropertyName("transfers")]
        public List<Transfer> Transfers { get; init; }
    }
}
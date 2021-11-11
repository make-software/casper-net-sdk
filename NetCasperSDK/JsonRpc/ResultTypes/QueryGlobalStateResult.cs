using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "query_global_state" RPC response.
    /// </summary>
    public class QueryGlobalStateResult : RpcResult
    {
        /// <summary>
        /// The block header if a Block hash was provided.
        /// </summary>
        [JsonPropertyName("block_header")]
        public BlockHeader BlockHeader { get; init; }
        
        /// <summary>
        /// The stored value.
        /// </summary>
        [JsonPropertyName("stored_value")]
        [JsonConverter(typeof(StoredValue.StoredValueConverter))]
        public StoredValue StoredValue { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }
}
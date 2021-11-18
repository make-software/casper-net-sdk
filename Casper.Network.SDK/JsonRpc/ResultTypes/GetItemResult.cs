using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_item" RPC response.
    /// </summary>
    public class GetItemResult : RpcResult
    {
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
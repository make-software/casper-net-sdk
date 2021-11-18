using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_account_info" RPC response.
    /// </summary>
    public class GetAccountInfoResult : RpcResult
    {
        /// <summary>
        /// The account returned.
        /// </summary>
        [JsonPropertyName("account")]
        public Account Account { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }
}
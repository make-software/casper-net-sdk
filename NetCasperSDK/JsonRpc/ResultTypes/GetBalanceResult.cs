using System.Numerics;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_balance" RPC response.
    /// </summary>
    public class GetBalanceResult : RpcResult
    {
        /// <summary>
        /// The balance value.
        /// </summary>
        [JsonPropertyName("balance_value")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger BalanceValue { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }
}
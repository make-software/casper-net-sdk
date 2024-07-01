using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "query_balance" RPC response.
    /// </summary>
    public class QueryBalanceResult : RpcResult
    {
        /// <summary>
        /// The balance value.
        /// </summary>
        [JsonPropertyName("balance")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger BalanceValue { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }
}
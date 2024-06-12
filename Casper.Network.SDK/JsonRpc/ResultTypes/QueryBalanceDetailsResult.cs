using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// A hold.
    /// </summary>
    public class Hold
    {
        /// <summary>
        /// The amount in the hold.
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// The block time at which the hold was created.
        /// </summary>
        [JsonPropertyName("time")]
        public UInt64 Time { get; init; }
        /// <summary>
        /// A proof that the given value is present in the Merkle trie.
        /// </summary>
        [JsonPropertyName("proof")]
        public string Proof { get; init; }
    }
    
    /// <summary>
    /// Result for "query_balance_details" RPC response.
    /// </summary>
    public class QueryBalanceDetailsResult : RpcResult
    {
        /// <summary>
        /// The purses total balance, not considering holds.
        /// </summary>
        [JsonPropertyName("total_balance")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger TotalBalance { get; init; }
        
        /// <summary>
        /// The available balance in motes (total balance - sum of all active holds).
        /// </summary>
        [JsonPropertyName("available_balance")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger AvailableBalance { get; init; }
        
        /// <summary>
        /// A proof that the given value is present in the Merkle trie.
        /// </summary>
        [JsonPropertyName("total_balance_proof")]
        public string TotalBalanceProof { get; init; }
        
        /// <summary>
        /// Holds active at the requested point in time.
        /// </summary>
        [JsonPropertyName("holds")]
        public List<Hold> Holds { get; init; }
    }
}

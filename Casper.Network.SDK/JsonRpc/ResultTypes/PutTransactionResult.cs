using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "account_put_transaction" RPC response.
    /// </summary>
    public class PutTransactionResult
    {
        /// <summary>
        /// Hex-encoded transaction hash.
        /// </summary>
        [JsonPropertyName("transaction_hash")]
        public TransactionHash TransactionHash { get; init; }
    }
}
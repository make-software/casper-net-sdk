using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    public class TransactionExpired
    {
        /// <summary>
        /// Versioned transaction hash.
        /// </summary>
        [JsonPropertyName("transaction_hash")]
        public TransactionHash TransactionHash { get; init; }
    }
}
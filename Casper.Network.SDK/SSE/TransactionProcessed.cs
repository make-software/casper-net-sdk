using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    public class TransactionProcessed
    {
        /// <summary>
        /// Versioned transaction hash.
        /// </summary>
        [JsonPropertyName("transaction_hash")]
        public TransactionHash TransactionHash { get; init; }

        /// <summary>
        /// The address of the initiator of a transaction.
        /// </summary>
        [JsonPropertyName("initiator_addr")]
        public InitiatorAddr InitiatorAddr { get; set; }

        /// <summary>
        /// The timestamp in which the <see cref="Transaction">Transaction</see> was built.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; }

        /// <summary>
        /// The time-to-live of the <see cref="Transaction">Transaction</see>.
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }

        /// <summary>
        /// The hash of the block containing this <see cref="Transaction">Transaction</see>.
        /// </summary>
        [JsonPropertyName("block_hash")] 
        public string BlockHash { get; init; }

        /// <summary>
        /// Versioned execution result.
        /// </summary>
        [JsonPropertyName("execution_result")]
        [JsonConverter(typeof(ExecutionResult.ExecutionResultConverter))]
        public ExecutionResult ExecutionResult { get; init; }
        
        /// <summary>
        /// List of messages emitted in the transaction execution
        /// </summary>
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; init; }
    }
}
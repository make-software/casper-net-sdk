using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A versioned wrapper for a transaction or deploy.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("Deploy")] 
        public Deploy Deploy { get; init; }
        
        /// <summary>
        /// A version 1 transaction.
        /// </summary>
        [JsonPropertyName("Version1")] 
        public TransactionV1 TransactionV1 { get; init; }
    }
}
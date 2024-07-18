using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Body of a TransactionV1.
    /// </summary>
    public class TransactionV1Body
    {
        /// <summary>
        /// List of runtime arguments.
        /// </summary>
        [JsonPropertyName("args")]
        [JsonConverter(typeof(GenericListConverter<NamedArg, NamedArg.NamedArgConverter>))]
        public List<NamedArg> RuntimeArgs { get; init; }
        
        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonPropertyName("entry_point")] 
        [JsonConverter(typeof(TransactionV1EntryPoint.TransactionEntryPointConverter))]
        public ITransactionV1EntryPoint EntryPoint { get; init; } 
        
        /// <summary>
        /// Target contract of the transaction (native, custom or session).
        /// </summary>
        [JsonPropertyName("target")] 
        [JsonConverter(typeof(TransactionV1Target.TransactionTargetConverter))]
        public ITransactionV1Target Target { get; init; }
        
        /// <summary>
        /// Scheduling of the transaction.
        /// </summary>
        [JsonPropertyName("scheduling")] 
        [JsonConverter(typeof(TransactionV1Scheduling.TransactionV1SchedulingConverter))]
        public ITransactionV1Scheduling Scheduling { get; init; }
        
        [JsonPropertyName("transaction_category")] 
        public TransactionCategory Category { get; init; }
    }
}

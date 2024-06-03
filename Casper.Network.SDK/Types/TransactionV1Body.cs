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
        [JsonConverter(typeof(TransactionEntryPoint.TransactionEntryPointConverter))]
        public TransactionEntryPoint EntryPoint { get; init; } 
        
        /// <summary>
        /// Target of the transaction (native, custom or module_bytes).
        /// </summary>
        [JsonPropertyName("target")] 
        [JsonConverter(typeof(TransactionTarget.TransactionTargetConverter))]
        public TransactionTarget Target { get; init; }
        
        /// <summary>
        /// Scheduling of the transaction..
        /// </summary>
        [JsonPropertyName("scheduling")] 
        [JsonConverter(typeof(TransactionScheduling.TransactionSchedulingConverter))]
        public TransactionScheduling Scheduling { get; init; }
        
        [JsonPropertyName("transaction_kind")] 
        public byte TransactionKind { get; init; }
    }
}

using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_deploy" RPC response.
    /// </summary>
    public class GetTransactionResult : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("transaction")]
        [JsonConverter(typeof(Transaction.TransactionConverter))]
        public Transaction Transaction { get; init; }

        /// <summary>
        /// Execution info, if available.
        /// </summary>
        [JsonPropertyName("execution_info")]
        public ExecutionInfo ExecutionInfo { get; init; }
    }
}

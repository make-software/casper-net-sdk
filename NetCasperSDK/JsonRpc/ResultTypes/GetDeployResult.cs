using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_deploy" RPC response.
    /// </summary>
    public class GetDeployResult : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("deploy")]
        public Deploy Deploy { get; init; }

        /// <summary>
        /// The map of block hash to execution result.
        /// </summary>
        [JsonPropertyName("execution_results")]
        [JsonConverter(typeof(GenericListConverter<ExecutionResult, ExecutionResult.ExecutionResultConverter>))]
        public List<ExecutionResult> ExecutionResults { get; init; }
    }
}

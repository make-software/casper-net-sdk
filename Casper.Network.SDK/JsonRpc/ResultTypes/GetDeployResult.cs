using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    public class GetDeployResultCompat : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("deploy")]
        public Deploy Deploy { get; init; }

        /// <summary>
        /// Execution info, if available.
        /// </summary>
        [JsonPropertyName("execution_info")]
        public ExecutionInfo ExecutionInfo { get; init; }

        /// <summary>
        /// The map of block hash to execution result.
        /// </summary>
        [JsonPropertyName("execution_results")]
        [JsonConverter(typeof(ExecutionResultV1.ExecutionResultV1Converter))]
        public ExecutionResultV1 ExecutionResult { get; init; }
    }

    /// <summary>
    /// Result for "info_get_deploy" RPC response.
    /// </summary>
    [JsonConverter(typeof(GetDeployResultConverter))]
    public class GetDeployResult : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("deploy")]
        public Deploy Deploy { get; init; }

        /// <summary>
        /// Execution info, if available.
        /// </summary>
        [JsonPropertyName("execution_info")]
        public ExecutionInfo ExecutionInfo { get; init; }

        public class GetDeployResultConverter : JsonConverter<GetDeployResult>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(GetDeployResult);
            }

            public override GetDeployResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var resultCompat = JsonSerializer.Deserialize<GetDeployResultCompat>(ref reader, options);

                    if (resultCompat.ExecutionResult != null)
                    {
                        return new GetDeployResult()
                        {
                            ApiVersion = resultCompat.ApiVersion,
                            Deploy = resultCompat.Deploy,
                            ExecutionInfo = new ExecutionInfo
                            {
                                BlockHash = resultCompat.ExecutionResult.BlockHash,
                                BlockHeight = 0,
                                ExecutionResult = (ExecutionResult)resultCompat.ExecutionResult,
                            },
                        };
                    }

                    return new GetDeployResult()
                    {
                        ApiVersion = resultCompat.ApiVersion,
                        Deploy = resultCompat.Deploy,
                        ExecutionInfo = resultCompat.ExecutionInfo,
                    };
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                GetDeployResult block,
                JsonSerializerOptions options)
            {
                throw new JsonException($"not yet implemented");
            }
        }
    }
}
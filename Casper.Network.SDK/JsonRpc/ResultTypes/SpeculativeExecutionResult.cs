using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "speculative_exec" RPC response.
    /// </summary>
    [JsonConverter(typeof(SpeculativeExecutionResultConverter))]
    public class SpeculativeExecutionResult : RpcResult
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The result of executing the <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("execution_result")]
        public ExecutionResult ExecutionResult { get; init; }

        /// <summary>
        /// List of messages emitted in the transaction execution
        /// </summary>
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; init; }

        public class SpeculativeExecutionResultConverter : JsonConverter<SpeculativeExecutionResult>
        {
            public override SpeculativeExecutionResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                using var document = JsonDocument.ParseValue(ref reader);
                var root = document.RootElement;
                if (!root.TryGetProperty("execution_result", out var execution) ||
                    execution.ValueKind != JsonValueKind.Object)
                    throw new JsonException("Expected an execution_result object");

                var apiVersion = root.TryGetProperty("api_version", out var version)
                    ? version.GetString()
                    : null;

                if (execution.TryGetProperty("Success", out _) || execution.TryGetProperty("Failure", out _))
                {
                    if (!root.TryGetProperty("block_hash", out var blockHash))
                        throw new JsonException("Expected a block_hash in the speculative execution result");

                    var v1Options = new JsonSerializerOptions(options);
                    v1Options.Converters.Add(new ExecutionResultV1.ExecutionResultV1Converter());
                    var v1 = JsonSerializer.Deserialize<ExecutionResultV1>(execution.GetRawText(), v1Options);
                    return new SpeculativeExecutionResult
                    {
                        ApiVersion = apiVersion,
                        BlockHash = blockHash.GetString(),
                        ExecutionResult = (ExecutionResult)v1,
                    };
                }

                if (execution.TryGetProperty("block_hash", out _))
                {
                    var v2 = JsonSerializer.Deserialize<SpeculativeExecutionResultV2>(execution.GetRawText(), options);
                    return new SpeculativeExecutionResult
                    {
                        ApiVersion = apiVersion,
                        BlockHash = v2.BlockHash,
                        ExecutionResult = ExecutionResult.FromSpeculativeExecutionV2(v2),
                        Messages = v2.Messages,
                    };
                }

                throw new JsonException("Unrecognized speculative execution result format");
            }

            public override void Write(
                Utf8JsonWriter writer,
                SpeculativeExecutionResult value,
                JsonSerializerOptions options)
            {
                throw new NotSupportedException("Serializing speculative execution results is not supported");
            }
        }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    public class DeployProcessed
    {
        [JsonPropertyName("deploy_hash")] public string DeployHash { get; init; }

        [JsonPropertyName("account")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Account { get; init; }

        /// <summary>
        /// The timestamp in which the fault occurred.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; }

        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }

        [JsonPropertyName("dependencies")] public List<byte[]> Dependencies { get; set; }

        [JsonPropertyName("block_hash")] public string BlockHash { get; init; }

        [JsonPropertyName("execution_result")]
        [JsonConverter(typeof(ExecutionResult.ExecutionResultConverter))]
        public ExecutionResult ExecutionResult { get; init; }
    }
}
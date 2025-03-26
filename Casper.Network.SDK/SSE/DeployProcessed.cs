using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A <see cref="Deploy">Deploy</see> that has been executed, committed and forms part of a <see cref="Block">Block</see>..
    /// </summary>
    public class DeployProcessed
    {
        /// <summary>
        /// The <see cref="Deploy">Deploy</see> hash.
        /// </summary>
        [JsonPropertyName("deploy_hash")] 
        public string DeployHash { get; init; }

        /// <summary>
        /// The public key of the account that originates the <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("account")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Account { get; init; }

        /// <summary>
        /// The timestamp in which the <see cref="Deploy">Deploy</see> was built.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; }

        /// <summary>
        /// The time-to-live of the <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }

        /// <summary>
        /// A list of hashes to other deploys that have to be run before this one.
        /// </summary>
        [JsonPropertyName("dependencies")] 
        public List<byte[]> Dependencies { get; set; }

        /// <summary>
        /// The hash of the block containing this <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("block_hash")] 
        public string BlockHash { get; init; }

        /// <summary>
        /// The result of executing a this <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("execution_result")]
        [JsonConverter(typeof(ExecutionResultV1.ExecutionResultV1Converter))]
        public ExecutionResultV1 ExecutionResult { get; init; }
    }
}
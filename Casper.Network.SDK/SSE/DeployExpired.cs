using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A <see cref="Deploy">Deploy</see> that has expired before being processed. 
    /// </summary>
    public class DeployExpired
    {
        /// <summary>
        /// The hash of the expired <see cref="Deploy">Deploy</see>.
        /// </summary>
        [JsonPropertyName("deploy_hash")] 
        public string DeployHash { get; init; }
    }
}
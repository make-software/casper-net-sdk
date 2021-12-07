using System.Text.Json.Serialization;

namespace Casper.Network.SDK.SSE
{
    public class DeployExpired
    {
        [JsonPropertyName("deploy_hash")] public string DeployHash { get; init; }
    }
}
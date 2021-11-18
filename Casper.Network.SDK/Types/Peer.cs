using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Network node
    /// </summary>
    public class Peer
    {
        /// <summary>
        /// Node Id
        /// </summary>
        [JsonPropertyName("node_id")] public string NodeId { get; init; }

        /// <summary>
        /// Node IP and port
        /// </summary>
        [JsonPropertyName("address")] public string Address { get; init; }
    }
}
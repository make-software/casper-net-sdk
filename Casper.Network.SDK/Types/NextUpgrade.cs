using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Information about the next protocol upgrade.
    /// </summary>
    public class NextUpgrade
    {
        //TODO: according to schema activation_point can be a Timestamp or an EraId (ie string or ulong)
        /// <summary>
        /// Era Id when the next upgrade will be activated.
        /// According to rpc schema, it can be also a Timestamp
        /// </summary>
        [JsonPropertyName("activation_point")] 
        public ulong ActivationPoint { get; init; }

        /// <summary>
        /// The protocol version of the next upgrade
        /// </summary>
        [JsonPropertyName("protocol_version")] 
        public string ProtocolVersion { get; init; }
    }
}
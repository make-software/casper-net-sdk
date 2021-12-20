using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// Generic representation of validator's fault in an era.
    /// </summary>
    public class Fault
    {
        /// <summary>
        /// The era in which the fault occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The timestamp in which the fault occurred.
        /// </summary>
        [JsonPropertyName("timestamp")] 
        public string Timestamp { get; init; }
        
        /// <summary>
        /// Equivocator public key
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }
    }
}
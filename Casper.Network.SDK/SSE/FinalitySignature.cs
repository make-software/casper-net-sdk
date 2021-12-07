using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// Server-Side event sent after each block finalization
    /// </summary>
    public class FinalitySignature
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }

        /// <summary>
        /// The block era id.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// Validator public key
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Validator signature
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }
}
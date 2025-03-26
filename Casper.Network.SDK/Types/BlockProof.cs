using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A validator's public key paired with a corresponding signature of a given block hash.
    /// </summary>
    public class BlockProof
    {
        /// <summary>
        /// The validator's public key.
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// The validator's signature.
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }
}
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Signature and Public Key of the signer.
    /// </summary>
    public class DeployApproval
    {
        /// <summary>
        /// Signature of a deploy.
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
        
        /// <summary>
        /// Public Key that generates the signature.
        /// </summary>
        [JsonPropertyName("signer")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Signer { get; init; }
    }
}
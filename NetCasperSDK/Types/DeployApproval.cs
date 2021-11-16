using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    public class DeployApproval
    {
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
        
        [JsonPropertyName("signer")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Signer { get; init; }
    }
}
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    public class DeployApproval
    {
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(SignatureConverter))]
        public Signature Signature { get; set; }
        
        [JsonPropertyName("signer")]
        public PublicKey Signer { get; set; }
    }
}
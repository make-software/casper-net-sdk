using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Container for bytes recording location, type and data for a gas pre-payment.
    /// </summary>
    public class Prepayment
    {
        [JsonPropertyName("receipt")]
        public string Receipt { get; init; }

        [JsonPropertyName("prepayment_kind")]
        public byte PrepaymentKind { get; init; }
        
        [JsonPropertyName("prepayment_data")]
        public string PrepaymentData { get; init; }
    }
}

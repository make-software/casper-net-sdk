using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Auction bid variants.
    /// </summary>
    public class BidKind
    {
        [JsonPropertyName("Unified")]
        public Bid Unified { get; init; }
        
        [JsonPropertyName("Validator")]
        public Bid Validator { get; init; }
        
        [JsonPropertyName("Delegator")]
        public Delegator Delegator { get; init; }
    }
}
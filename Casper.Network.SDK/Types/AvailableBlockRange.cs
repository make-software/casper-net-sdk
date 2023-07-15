using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// An unbroken, inclusive range of blocks.
    /// </summary>
    public class AvailableBlockRange
    {
        /// <summary>
        /// The inclusive upper bound of the range.
        /// </summary>
        [JsonPropertyName("high")]
        public ulong High { get; init; }    
        
        /// <summary>
        /// The inclusive lower bound of the range.
        /// </summary>
        [JsonPropertyName("low")]
        public ulong Low { get; init; }
    }
}

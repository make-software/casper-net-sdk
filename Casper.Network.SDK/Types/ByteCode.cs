using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A container for contract's Wasm bytes.
    /// </summary>
    public class ByteCode
    {
        [JsonPropertyName("kind")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ByteCodeKind Kind { get; init; }
        
        [JsonPropertyName("bytes")]
        public string Bytes { get; init; }
    }
}
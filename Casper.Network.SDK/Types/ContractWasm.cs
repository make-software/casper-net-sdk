using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A container for contract's WASM bytes.
    /// </summary>
    public class ContractWasm
    {
        [JsonPropertyName("bytes")]
        public string Bytes { get; init; }
    }
}
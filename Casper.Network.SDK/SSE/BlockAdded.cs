using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    public class BlockAdded
    {
        [JsonPropertyName("block_hash")] public string BlockHash { get; init; }

        [JsonPropertyName("block")] public Block Block { get; init; }
    }
}
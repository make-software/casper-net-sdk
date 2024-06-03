using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A <see cref="Block">Block</see> that has been added to the linear chain and stored in the node.
    /// </summary>
    public class BlockAdded
    {
        /// <summary>
        /// The <see cref="Block">Block</see> hash.
        /// </summary>
        [JsonPropertyName("block_hash")] 
        public string BlockHash { get; init; }

        /// <summary>
        /// The <see cref="Block">Block</see> data.
        /// </summary>
        [JsonPropertyName("block")]
        [JsonConverter(typeof(Block.BlockConverter))]
        public Block Block { get; init; }
    }
}
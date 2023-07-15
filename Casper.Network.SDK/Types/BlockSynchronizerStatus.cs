using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The status of the block synchronizer.
    /// </summary>
    public class BlockSynchronizerStatus
    {
        /// <summary>
        /// The status of syncing a historical block, if any.
        /// </summary>
        [JsonPropertyName("historical")]
        public BlockSyncStatus Historical { get; init; }
        
        /// <summary>
        /// The status of syncing a forward block, if any.
        /// </summary>
        [JsonPropertyName("forward")]
        public BlockSyncStatus Forward { get; init; }
    }
    
    /// <summary>
    /// The status of syncing an individual block.
    /// </summary>
    public class BlockSyncStatus
    {
        /// <summary>
        /// The block hash.
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The block hash.
        /// </summary>
        [JsonPropertyName("block_height")]
        public ulong? BlockHeight { get; init; }

        /// <summary>
        /// The state of acquisition of the data associated with the block.
        /// </summary>
        [JsonPropertyName("acquisition_state")]
        public string AcquisitionState { get; init; }
    }
}

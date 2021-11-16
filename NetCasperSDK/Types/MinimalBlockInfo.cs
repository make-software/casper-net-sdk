using System.Text.Json.Serialization;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Minimal info of a `Block`.
    /// </summary>
    public class MinimalBlockInfo
    {
        /// <summary>
        /// Block hash
        /// </summary>
        [JsonPropertyName("hash")] public string Hash { get; init; }

        /// <summary>
        /// The block timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")] public string Timestamp { get; init; }

        /// <summary>
        /// The block era id.
        /// </summary>
        [JsonPropertyName("era_id")] public ulong EraId { get; init; }

        /// <summary>
        /// Height of the block
        /// </summary>
        [JsonPropertyName("height")] public ulong Height { get; init; }

        /// <summary>
        /// The state root hash.
        /// </summary>
        [JsonPropertyName("state_root_hash")] public string StateRootHash { get; init; }

        /// <summary>
        /// Validator node's public key
        /// </summary>
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        [JsonPropertyName("creator")] public PublicKey PublicKey { get; init; }
    }
}
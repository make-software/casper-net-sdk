using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_status" RPC response.
    /// </summary>
    public class GetNodeStatusResult : RpcResult
    {
        /// <summary>
        /// The chainspec name.
        /// </summary>
        [JsonPropertyName("chainspec_name")] public string ChainspecName { get; init; }

        /// <summary>
        /// The state root hash used at the start of the current session.
        /// </summary>
        [JsonPropertyName("starting_state_root_hash")]
        public string StartingStateRootHash { get; init; }

        /// <summary>
        /// The node ID and network address of each connected peer.
        /// </summary>
        [JsonPropertyName("peers")] public List<Peer> Peers { get; init; }

        /// <summary>
        /// The minimal info of the last block from the linear chain.
        /// </summary>
        [JsonPropertyName("last_added_block_info")]
        public MinimalBlockInfo LastAddedBlockInfo { get; init; }

        /// <summary>
        /// Node public signing key.
        /// </summary>
        [JsonPropertyName("our_public_signing_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey OurPublicSigningKey { get; init; }

        /// <summary>
        /// Information about the next scheduled upgrade.
        /// </summary>
        [JsonPropertyName("next_upgrade")] public NextUpgrade NextUpgrade { get; init; }
        
        /// <summary>
        /// The next round length if this node is a validator.
        /// </summary>
        [JsonPropertyName("round_length")] public string RoundLength { get; init; }

        /// <summary>
        /// The compiled node version.
        /// </summary>
        [JsonPropertyName("build_version")] public string BuildVersion { get; init; }

        /// <summary>
        /// Time that passed since the node has started.
        /// </summary>
        [JsonPropertyName("uptime")] public string Uptime { get; init; }
    }
}
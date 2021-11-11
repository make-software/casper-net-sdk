using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_peers" RPC response.
    /// </summary>
    public class GetNodePeersResult : RpcResult
    {
        /// <summary>
        /// The node ID and network address of each connected peer.
        /// </summary>
        [JsonPropertyName("peers")] public List<Peer> Peers { get; init; }
    }
}
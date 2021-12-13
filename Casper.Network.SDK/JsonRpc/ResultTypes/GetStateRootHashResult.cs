using System.Text.Json.Serialization;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for the "chain_get_state_root_hash" RPC.
    /// </summary>
    public class GetStateRootHashResult : RpcResult
    {
        /// <summary>
        /// Hex-encoded hash of the state root.
        /// </summary>
        [JsonPropertyName("state_root_hash")] 
        public string StateRootHash { get; init; }
    }
}
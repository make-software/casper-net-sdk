using System.Text.Json.Serialization;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    public class GetStateRootHashResult : RpcResult
    {
        [JsonPropertyName("state_root_hash")] public string StateRootHash { get; init; }
    }
}
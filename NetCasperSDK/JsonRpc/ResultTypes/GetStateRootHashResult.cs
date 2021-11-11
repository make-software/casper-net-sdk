using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    public class GetStateRootHashResult : RpcResult
    {
        [JsonPropertyName("state_root_hash")] public string StateRootHash { get; init; }
    }
}
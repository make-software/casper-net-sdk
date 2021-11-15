using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc
{
    public class RpcError
    {
        [JsonPropertyName("code")] public int Code { get; init; }
        [JsonPropertyName("message")] public string Message { get; init; }
        [JsonPropertyName("data")] public JsonElement Data { get; init; }
    }
}
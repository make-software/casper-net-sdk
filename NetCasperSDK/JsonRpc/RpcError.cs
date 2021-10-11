using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc
{
    public class RpcError
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("data")] public JsonElement Data { get; set; }
    }
}
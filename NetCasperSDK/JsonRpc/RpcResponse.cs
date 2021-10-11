using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc
{
    public class RpcResponse
    {
        [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; }

        [JsonPropertyName("id")] public uint Id { get; set; }

        [JsonPropertyName("result")] public JsonElement Result { get; set; }

        [JsonPropertyName("error")] public RpcError Error { get; set; }
    }
}
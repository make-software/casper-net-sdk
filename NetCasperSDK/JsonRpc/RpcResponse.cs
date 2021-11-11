using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetCasperSDK.JsonRpc
{
    public class RpcResponse<TRpcResult>
    {
        [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; }

        [JsonPropertyName("id")] public uint Id { get; set; }

        [JsonPropertyName("result")] public JsonElement Result { get; set; }

        [JsonPropertyName("error")] public RpcError Error { get; set; }

        public TRpcResult Parse()
        {
            return JsonSerializer.Deserialize<TRpcResult>(this.Result.GetRawText());
        }
    }
}
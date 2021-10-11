using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.JsonRpc
{
    public class RpcMethod
    {
        [JsonPropertyName("jsonrpc")] public string JsonRpc { get; } = "2.0";

        [JsonPropertyName("id")] public uint Id { get; private set; } = 1;

        [JsonPropertyName("method")] public string Method { get; private set; }

        [JsonPropertyName("params")] public Dictionary<string, object> Parameters { get; protected set; }

        public RpcMethod(string method, Dictionary<string, object> parameters = null)
        {
            this.Method = method;
            this.Parameters = parameters;
        }

        public string Serialize()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new PublicKeyConverter(),
                    new NamedArgConverter(),
                    new ExecutableDeployItemConverter()
                }
            };
            return JsonSerializer.Serialize(this, typeof(RpcMethod), options);
        }
    }
}
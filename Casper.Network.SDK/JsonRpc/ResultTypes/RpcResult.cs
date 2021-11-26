using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Base class for all/most typed RPC responses
    /// </summary>
    public class RpcResult
    {
        /// <summary>
        /// The RPC API version.
        /// </summary>
        [JsonPropertyName("api_version")] public string ApiVersion { get; init; }

        public static T Load<T>(string file) where T : RpcResult
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
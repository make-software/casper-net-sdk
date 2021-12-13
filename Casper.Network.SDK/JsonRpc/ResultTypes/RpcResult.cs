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

        /// <summary>
        /// Loads and deserializes an RPC response from a file.
        /// </summary>
        /// <typeparam name="T">Class, deriving from RpcResult, used for the deserialization</typeparam>
        public static T Load<T>(string file) where T : RpcResult
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(json);
        }
        
        /// <summary>
        /// Deserializes an RPC response from a string.
        /// </summary>
        /// <typeparam name="T">Class, deriving from RpcResult, used for the deserialization</typeparam>
        public static T Parse<T>(string json) where T : RpcResult
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
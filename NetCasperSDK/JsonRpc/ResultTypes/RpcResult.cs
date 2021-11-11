using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc.ResultTypes
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
    }
}
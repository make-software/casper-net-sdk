using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "chain_get_era_info" RPC response.
    /// </summary>
    public class GetEraInfoBySwitchBlockResult : RpcResult
    {
        [JsonPropertyName("era_summary")]
        public EraSummary EraSummary { get; init; }
    }
}
using System.Text.Json.Serialization;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "account_put_deploy" RPC response.
    /// </summary>
    public class PutDeployResult : RpcResult
    {
        /// <summary>
        /// Hex-encoded deploy hash.
        /// </summary>
        [JsonPropertyName("deploy_hash")]
        public string DeployHash { get; init; }
    }
}
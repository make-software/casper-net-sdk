using System.Text.Json;
using NetCasperSDK.JsonRpc;

namespace NetCasperSDK.Utils
{
    public static class RpcResponseExtensions
    {
        public static string GetDeployHash(this RpcResponse response)
        {
            if (response.Result.TryGetProperty("deploy_hash", out var el))
                return el.GetString();

            throw new RpcClientException("deploy_hash property not found.");
        }
    }
}
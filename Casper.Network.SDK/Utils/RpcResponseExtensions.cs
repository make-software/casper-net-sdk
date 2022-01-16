using Casper.Network.SDK.JsonRpc;

namespace Casper.Network.SDK.Utils
{
    public static class RpcResponseExtensions
    {
        /// <summary>
        /// Retrieves the Deploy hash from an RpcResponse object.
        /// </summary>
        public static string GetDeployHash<TRpcResult>(this RpcResponse<TRpcResult> response)
        {
            if (response.Result.TryGetProperty("deploy_hash", out var el))
                return el.GetString();

            throw new RpcClientException("deploy_hash property not found.");
        }
    }
}
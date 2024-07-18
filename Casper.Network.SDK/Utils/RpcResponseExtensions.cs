using System.Text.Json;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

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
        
        /// <summary>
        /// Retrieves the Transaction hash from an RpcResponse object.
        /// </summary>
        public static TransactionHash GetTransactionHash<TRpcResult>(this RpcResponse<TRpcResult> response)
        {
            if (response.Result.TryGetProperty("transaction_hash", out var el))
            {
                if (el.ValueKind != JsonValueKind.Null)
                {
                    var hash = JsonSerializer.Deserialize<TransactionHash>(el.GetRawText());
                    if (hash != null)
                        return hash;
                    
                    throw new RpcClientException("Cannot deserialize transaction_hash property in response.");
                }
            }

            throw new RpcClientException("transaction_hash property not found.");
        }
    }
}
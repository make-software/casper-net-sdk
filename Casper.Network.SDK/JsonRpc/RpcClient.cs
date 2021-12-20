using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Casper.Network.SDK.JsonRpc
{
    /// <summary>
    /// The RPC client to send and receive data to the Casper network.
    /// </summary>
    public class RpcClient
    {
        private HttpClient httpClient;

        /// <summary>
        /// Creates an instance of the RPC Client that connects to the node address received
        /// as argument. Optionally, use an RpcLoggingHandler object to log requests and responses.
        /// </summary>
        public RpcClient(string nodeAddress, RpcLoggingHandler loggingHandler = null)
        {
            if (loggingHandler != null)
                httpClient = new HttpClient(loggingHandler);
            else
                httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(nodeAddress);
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header
        }

        /// <summary>
        /// Sends the RPC method received as argument to the network and returns an RPC response object.
        /// </summary>
        public async Task<RpcResponse<TRpcResult>> SendRpcRequestAsync<TRpcResult>(RpcMethod method)
        {
            var rpcResponse = await SendAsync<TRpcResult>(method);

            if (rpcResponse.Error != null)
                throw new RpcClientException("Error in request. Check inner RpcError object.", rpcResponse.Error);
            
            return rpcResponse;
        }

        protected async Task<RpcResponse<TRpcResult>> SendAsync<TRpcResult>(RpcMethod method)
        {
            var strMethod = method.Serialize();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "rpc");

            request.Content = new StringContent(
                strMethod,
                System.Text.Encoding.UTF8,
                "application/json"); //CONTENT-TYPE header
            if(request.Content.Headers.ContentType != null)
                request.Content.Headers.ContentType.CharSet = "";

            try
            {
                var responseMessage = await httpClient.SendAsync(request);
                responseMessage.EnsureSuccessStatusCode();
                
                var jsonResponse = await responseMessage.Content.ReadFromJsonAsync<RpcResponse<TRpcResult>>();
                return jsonResponse;
            }
            catch (Exception e)
            {
                throw new RpcClientException(e.Message, e);
            }
        }
        
        public async Task<RpcResponse<TRpcResult>> SendJsonAsync<TRpcResult>(string json)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "rpc");

            request.Content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"); //CONTENT-TYPE header
            if(request.Content.Headers.ContentType != null)
                request.Content.Headers.ContentType.CharSet = "";

            try
            {
                var responseMessage = await httpClient.SendAsync(request);
                responseMessage.EnsureSuccessStatusCode();
                
                var jsonResponse = await responseMessage.Content.ReadFromJsonAsync<RpcResponse<TRpcResult>>();
                return jsonResponse;
            }
            catch (Exception e)
            {
                throw new RpcClientException(e.Message, e);
            }
        }
    }
}
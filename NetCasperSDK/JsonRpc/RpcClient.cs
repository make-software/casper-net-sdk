using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NetCasperSDK.JsonRpc
{
    public class RpcClient
    {
        private HttpClient httpClient;

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

        public async Task<RpcResponse> SendRpcRequestAsync(RpcMethod method)
        {
            var rpcResponse = await SendAsync(method);

            if (rpcResponse.Error != null)
                throw new RpcClientException("Error in request. Check inner RpcError object.", rpcResponse.Error);
            
            return rpcResponse;
        }

        protected async Task<RpcResponse> SendAsync(RpcMethod method)
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
                
                var jsonResponse = await responseMessage.Content.ReadFromJsonAsync<RpcResponse>();
                return jsonResponse;
            }
            catch (Exception e)
            {
                throw new RpcClientException(e.Message, e);
            }
        }
    }
}
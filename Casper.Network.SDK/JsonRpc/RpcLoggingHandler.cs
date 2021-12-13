using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Casper.Network.SDK.JsonRpc
{
    /// <summary>
    /// Attach an RpcLoggingHandler to an instance of the RpcClient class to log
    /// request and response data to a writer stream.
    /// </summary>
    public class RpcLoggingHandler : DelegatingHandler
    {
        public StreamWriter LoggerStream { get; init; }

        public RpcLoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        private void Log(string line)
        {
            if (LoggerStream != null)
            {
                LoggerStream.WriteLine(line);
                LoggerStream.Flush();
            }
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Log("Request:");
            Log(request.ToString());
            if (request.Content != null && LoggerStream!=null)
            {
               Log(await request.Content.ReadAsStringAsync());
            }
            Log(string.Empty);

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }

            Log("Response:");
            Log(response.ToString());
            if (LoggerStream!=null)
            {
                Log(await response.Content.ReadAsStringAsync());
            }
            Log(string.Empty);

            return response;
        }
    }
}
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace Casper.NET.SDK.Examples
{
    public class GetNodeStatus
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://127.0.0.1:11101/rpc";

            try
            {
                // create an instance of the NetCasperClient that logs requests/outputs in stdout
                //
                var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
                {
                    LoggerStream = new StreamWriter(Console.OpenStandardOutput())
                };
                var casperSdk = new NetCasperClient(nodeAddress, loggingHandler);
                
                // get node status and print API version and PK as example
                //
                var rpcResponse = await casperSdk.GetNodeStatus();
                var nodeStatus = rpcResponse.Parse();
                
                Console.WriteLine("API Version : " + nodeStatus.ApiVersion);
                Console.WriteLine("Node PK     : " + nodeStatus.OurPublicSigningKey);
            }
            catch (RpcClientException e)
            {
                Console.WriteLine("ERROR:\n" + e.RpcError.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
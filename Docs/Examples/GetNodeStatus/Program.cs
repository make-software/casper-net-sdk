using System;
using System.Net.Http;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.NET.SDK.Examples
{
    public class GetNodeStatus
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://52.35.59.254:7777/rpc";
            
            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);
                var rpcResponse = await casperSdk.GetNodeStatus();
                Console.WriteLine(rpcResponse.Result.GetRawText());
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
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
            string nodeAddress = "http://127.0.0.1:11101/rpc";
            
            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);
                var rpcResponse = await casperSdk.QueryBalance(PublicKey.FromHexString("01Fed662DC7F1f7Af43Ad785Ba07a8cc05b7a96F9EE69613CfdE43BC56bEC1140B"));
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
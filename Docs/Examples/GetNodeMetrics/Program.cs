using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.NET.SDK.Examples
{
    public class GetNodeMetrics
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://testnet-node.make.services:8888/metrics";

            try
            {
                var metrics = await NetCasperClient.GetNodeMetrics(nodeAddress);
                Console.WriteLine(metrics);
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
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.NET.SDK.Examples
{
    public class GetBlockTransfers
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://52.35.59.254:7777/rpc";
            string blockHash = "c7148e1e2e115d8fba357e04be2073d721847c982dc70d5c36b5f6d3cf66331c";
            int blockHeight = 20652;

            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);

                //Get latest block
                var rpcResponse = await casperSdk.GetBlockTransfers();
                var transfers = rpcResponse.Parse().Transfers;
                Console.WriteLine("Number of transfers in latest block: " + transfers.Count);
                
                //Get block by hash
                rpcResponse = await casperSdk.GetBlockTransfers(blockHash);
                
                // //Get block by height
                rpcResponse = await casperSdk.GetBlockTransfers(blockHeight);
                transfers = rpcResponse.Parse().Transfers;

                Console.WriteLine($"Number of transfers in block {blockHeight}: " + transfers.Count);

                foreach (var transfer in transfers)
                {
                    Console.WriteLine("Transfer amount: " + transfer.Amount);
                }
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
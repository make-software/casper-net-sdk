using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace Casper.NET.SDK.Examples
{
    public class GetBlockTransfers
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://127.0.0.1:11101/rpc";
            
            // find a block height with transfers using a block explorer
            ulong blockHeight = 222; 

            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);

                // Get latest block transfers
                var rpcResponse = await casperSdk.GetBlockTransfers();
                var transfers = rpcResponse.Parse().Transfers;
                Console.WriteLine("Number of transfers in latest block: " + transfers.Count);
                
                // Get block transfers by block height
                rpcResponse = await casperSdk.GetBlockTransfers(blockHeight);
                transfers = rpcResponse.Parse().Transfers;

                Console.WriteLine($"Number of transfers in block {blockHeight}: " + transfers.Count);

                foreach (var transfer in transfers)
                {
                    Console.WriteLine("Transfer amount: " + transfer.Amount);
                    Console.WriteLine("Transfer from: " + transfer.From);
                    Console.WriteLine("Transfer to: " + transfer.To);
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
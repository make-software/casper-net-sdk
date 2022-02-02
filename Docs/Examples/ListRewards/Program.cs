using System;
using System.Linq;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace ListRewards
{
    public static class ListRewards
    {
        static string nodeAddress = "http://testnet-node.make.services:7777/rpc";
        static NetCasperClient casperSdk;

        public async static Task GetEraSummary()
        {
            var rpcResponse = await casperSdk.GetEraInfoBySwitchBlock(435_733);
            var eraSummary = rpcResponse.Parse().EraSummary;

            Console.WriteLine("Block Hash: " + eraSummary.BlockHash);
            Console.WriteLine("Era Id    : " + eraSummary.EraId);

            var eraInfo = eraSummary.StoredValue.EraInfo;

            // print the era rewards per validator
            //
            Console.WriteLine("Validator                                                            Deleg.    Rewards");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");

            var groupedByValidator = eraInfo.SeigniorageAllocations
                .GroupBy(allocation => allocation.ValidatorPublicKey);

            foreach (var group in groupedByValidator)
            {
                var eraRewards = group.Sum(a => (double)a.Amount);
                eraRewards /= 1_000_000_000;

                Console.WriteLine($"{group.Key} - {group.Count(),5} - " +
                                  $"{eraRewards.ToString("N9"),20} $CSPR");
            }
        }

        public async static Task Main(string[] args)
        {
            casperSdk = new NetCasperClient(nodeAddress);

            try
            {
                await GetEraSummary();
            }
            catch (RpcClientException e)
            {
                Console.WriteLine(e.RpcError.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace Casper.NET.SDK.Examples
{
    public static class ListRewards
    {
        static string nodeAddress = "http://127.0.0.1:11101/rpc";

        static NetCasperClient casperSdk;

        public async static Task GetEraSummary()
        {
            var rpcResponse = await casperSdk.GetEraSummary();
            var eraSummary = rpcResponse.Parse().EraSummary;

            Console.WriteLine("Block Hash: " + eraSummary.BlockHash);
            Console.WriteLine("Era Id    : " + eraSummary.EraId);

            var eraInfo = eraSummary.StoredValue.EraInfo;

            // print the era rewards per validator
            //
            Console.WriteLine("Validator");
            Console.WriteLine("  Delegator                                                            Rewards");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");

            var groupedByValidator = eraInfo.SeigniorageAllocations
                .GroupBy(allocation => allocation.ValidatorPublicKey);

            foreach (var group in groupedByValidator)
            {
                var rewards = group.Where(a => !a.IsDelegator).Sum(a => (double)a.Amount);
                rewards /= 1_000_000_000;

                Console.WriteLine($"{group.Key} " +
                                  $"{rewards.ToString("N9"),35} $CSPR");

                var delegators = group.Where(a => a.IsDelegator)
                    .GroupBy(a => a.DelegatorPublicKey);
                foreach (var delegatorAllocations in delegators)
                {
                    var delegatorRewards = delegatorAllocations.Sum(a =>  (double)a.Amount);
                    Console.WriteLine("  " + delegatorAllocations.Key + "  " +
                                      $"{delegatorRewards.ToString("N9"),32} $CSPR");
                }
                Console.WriteLine();
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
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.NET.SDK.Examples
{
    public class GetBalance
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://127.0.0.1:11101/rpc";

            var hex = "0184f6d260F4EE6869DDB36affe15456dE6aE045278FA2f467bb677561cE0daD55";
            var publicKey = PublicKey.FromHexString(hex);

            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);

                // Get the balance using the account public key
                //
                var rpcResponse = await casperSdk.QueryBalance(publicKey);
                Console.WriteLine("Public Key Balance : " + rpcResponse.Parse().BalanceValue);

                // Alternatively, use the main purse URef key to get the balance
                //
                var mainPurse = new URef("uref-0d0b57865e41b9e39170c038993997af432f66545f56838f1bf602c6d56e0e54-007");
                var purseResponse = await casperSdk.QueryBalance(mainPurse);
                Console.WriteLine("Purse Balance      : " + purseResponse.Parse().BalanceValue);

                // Or using an entity key
                //
                var entity = new AddressableEntityKey(
                    "entity-account-56befc13a6fd62e18f361700a5e08f966901c34df8041b36ec97d54d605c23de");
                var entityResponse = await casperSdk.QueryBalance(entity);
                Console.WriteLine("Entity Balance     : " + entityResponse.Parse().BalanceValue);
            }
            catch (RpcClientException e)
            {
                Console.WriteLine("ERROR:\n" + e.RpcError.Data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
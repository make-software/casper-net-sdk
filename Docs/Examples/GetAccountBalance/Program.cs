using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.NET.SDK.Examples
{
    public class GetAccountBalance
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://3.136.227.9:7777/rpc";

            var hex = "0203914289b334f57366541099a52156b149436fdb0422b3c48fe4115d0578abf690";
            var publicKey = PublicKey.FromHexString(hex);

            try
            {
                var casperSdk = new NetCasperClient(nodeAddress);

                // Get the balance using the account public key
                //
                var rpcResponse = await casperSdk.GetAccountBalance(publicKey);
                Console.WriteLine("Public Key Balance: " + rpcResponse.Parse().BalanceValue);

                // Alternatively, use the main purse URef key to get the balance
                //
                var mainPurse = "uref-91E6278EA0CCC22EAc98d1E6f2C6530f1e8a278Eb1005c4B36E070B400C88301-007";
                var purseResponse = await casperSdk.GetAccountBalance(mainPurse);
                Console.WriteLine("Purse Balance     : " + purseResponse.Parse().BalanceValue);
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
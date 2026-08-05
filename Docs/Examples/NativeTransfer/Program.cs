using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.NET.SDK.Examples
{
    public class NativeTransfer
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "https://node.testnet.casper.network/rpc";
            string chainName = "casper-test";
            
            try
            {
                // create an instance of the NetCasperClient that logs requests/outputs in stdout
                //
                var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
                {
                    LoggerStream = new StreamWriter(Console.OpenStandardOutput())
                };
                var casperSdk = new NetCasperClient(nodeAddress, loggingHandler);
                
                // load source account secret key and target public key from PEM files
                //
                var sourceKey = KeyPair.FromPem("./testnet1/secret_key.pem");
                var targetPK = PublicKey.FromPem("./testnet2/public_key.pem");

                // prepare a transfer transaction using the transaction builder.
                //
                var transaction = new Transaction.NativeTransferBuilder()
                    .From(sourceKey.PublicKey)
                    .Target(targetPK)
                    .Amount(2_500_000_000)
                    .Id(DateUtils.ToEpochTime(DateTime.Now))
                    .ChainName(chainName)
                    .Payment(100_000_000)
                    .Build();

                // sign the transaction and send it to the network
                // use the origin account secret key for signing.
                //
                transaction.Sign(sourceKey);

                var response = await casperSdk.PutTransaction(transaction);

                // extract the transaction hash and use it to wait (up to 2mins) for the execution results
                //
                var transactionHash = response.GetTransactionHash();
                
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                await casperSdk.GetTransaction(transactionHash, tokenSource.Token);

                Console.WriteLine("Transaction executed. See the results: https://testnet.cspr.live/transaction/" + transactionHash);
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

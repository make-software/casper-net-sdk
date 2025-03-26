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
            string nodeAddress = "http://127.0.0.1:11101/rpc";
            string chainName = "casper-net-1";
            
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
                    .Amount(25_000_000_000)
                    .Id(DateUtils.ToEpochTime(DateTime.Now))
                    .ChainName(chainName)
                    .Payment(PricingMode.PaymentLimited(100_000_000, 1))
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
                var transactionResponse = await casperSdk.GetTransaction(transactionHash, tokenSource.Token);

                // only as an example, extract the transfer key, and retrieve the transfer
                // information from the network
                //
                var transfer = transactionResponse.Parse().ExecutionInfo.ExecutionResult.Transfers[0];
                
                Console.WriteLine("Transfer amount: " + 
                                  transfer.Amount);
                Console.WriteLine("Transfer from  : " + 
                                  transfer.From);
                Console.WriteLine("Transfer to    : " + 
                                  transfer.To);
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

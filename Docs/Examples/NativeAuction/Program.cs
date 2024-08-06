using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.NET.SDK.Examples
{
    public class NativeAuction
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
                
                // load delegator account secret key from PEM files
                //
                var delegator = KeyPair.FromPem("./testnet1/secret_key.pem");

                // choose a validator public key to stake the tokens
                //
                var validatorPK = PublicKey.FromHexString("01509254f22690fbe7fb6134be574c4fbdb060dfa699964653b99753485e518ea6");
                
                // prepare a delegate transaction using the transaction builder.
                //
                var transaction = new Transaction.NativeDelegateBuilder()
                    .From(delegator.PublicKey)
                    .Validator(validatorPK)
                    .Amount(1000_000_000_000)
                    .ChainName(chainName)
                    .Build();
        
                // sign the transaction and send it to the network
                // use the delegator account secret key for signing.
                //
                transaction.Sign(delegator);

                var response = await casperSdk.PutTransaction(transaction);

                // extract the transaction hash and use it to wait (up to 2mins) for the execution results
                //
                var transactionHash = response.GetTransactionHash();
                
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var transactionResponse = await casperSdk.GetTransaction(transactionHash, tokenSource.Token);

                var executionResult = transactionResponse.Parse().ExecutionInfo.ExecutionResult;

                if (executionResult.IsSuccess)
                {
                    Console.WriteLine("Delegation successful");

                    // only as an example, extract the Bid transform,
                    // and print the staked amount and the bonding purse
                    //
                    var transform = executionResult.Effect.First(t => t.Kind is WriteTransformKind kind &&
                                                                      kind.Value.BidKind is not null);

                    var bid = (transform.Kind as WriteTransformKind).Value.BidKind;
                    Console.WriteLine("Staked amount : " + bid.Delegator.StakedAmount);
                    Console.WriteLine("Bonding purse : " + bid.Delegator.BondingPurse.ToString());
                }
                else
                    Console.WriteLine("Delegation completed with errors: " + executionResult.ErrorMessage);
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

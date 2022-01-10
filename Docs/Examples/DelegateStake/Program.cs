using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace CasperIntegrations
{
    public class DelegateStake
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://testnet-node.make.services:7777/rpc";

            try
            {
                // create an instance of the NetCasperClient that logs requests/outputs in stdout
                //
                var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
                {
                    LoggerStream = new StreamWriter(Console.OpenStandardOutput())
                };
                var casperSdk = new NetCasperClient(nodeAddress, loggingHandler);
                
                // load source account secret key from PEM files
                //
                var sourceKey = KeyPair.FromPem("./testnet1/secret_key.pem");

                // choose a validator public key to stake the tokens
                //
                var validatorPK = PublicKey.FromHexString("017d96b9a63abcb61c870a4f55187a0a7ac24096bdb5fc585c12a686a4d892009e");
                
                // create a hash key with the auction contract hash
                // IMPORTANT: this value changes between network. Double check you 
                // are using the right contract hash before executing the deploy!
                //
                var contractHash = GlobalStateKey.FromString("hash-93d923e336b20a4c4ca14d592b60e5bd3fe330775618290104f9beb326db7ae2") as HashKey;

                // prepare a transfer deploy using the DelegateTokens template.
                // Similarly, use the template UndelegateTokens to undelegate.
                //
                var deploy = DeployTemplates.DelegateTokens(
                    contractHash,
                    sourceKey.PublicKey,
                    validatorPK,
                    100_000_000_000,
                    3_000_000_000,
                    "casper-test");

                // sign the deploy and send it to the network
                // use the source account secret key for signing.
                //
                deploy.Sign(sourceKey);

                await casperSdk.PutDeploy(deploy);

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

                // only as an example, extract the transfer key, and retrieve the transfer
                // information from the network
                //
                var result = deployResponse.Parse();
                
                if(result.ExecutionResults[0].IsSuccess)
                    Console.WriteLine("Delegation successful");
                else
                    Console.WriteLine("Delegation completed with errors: " + result.ExecutionResults[0].ErrorMessage);
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

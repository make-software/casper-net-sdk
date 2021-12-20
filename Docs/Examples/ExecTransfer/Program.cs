using System;
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
    public class ExecAccountTransfer
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://3.136.227.9:7777/rpc";

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

                // prepare a transfer deploy using the StandardTransfer template.
                //
                var deploy = DeployTemplates.StandardTransfer(
                    sourceKey.PublicKey,
                    targetPK,
                    25_000_000_000,
                    100_000_000,
                    "casper-test");

                // sign the deploy and send it to the network
                // use the origin account secret key for signing.
                //
                deploy.Sign(sourceKey);

                var response = await casperSdk.PutDeploy(deploy);

                // extract the deploy hash and use it to wait (up to 2mins) for the execution results
                //
                var deployHash = response.GetDeployHash();
                
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

                // only as an example, extract the transfer key, and retrieve the transfer
                // information from the network
                //
                var transferKey = deployResponse.Parse().ExecutionResults[0].Transfers[0];
                
                var queryResponse = await casperSdk.QueryGlobalState(transferKey);
                var transfer = queryResponse.Parse().StoredValue.Transfer;
                
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

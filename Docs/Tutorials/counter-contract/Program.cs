using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Tutorials
{
    public static class DeployCounterContract
    {
        static string nodeAddress = "http://127.0.0.1:11101/rpc";
        static string chainName = "casper-net-1";

        static NetCasperClient casperSdk;

        static KeyPair faucetAcct = KeyPair.FromPem("./faucetact.pem");

        static KeyPair myAccount = KeyPair.FromPem("./myaccount.pem");

        static PublicKey myAccountPK = PublicKey.FromPem("./myaccount_pk.pem");

        public static async Task FundAccount()
        {
            var deploy = DeployTemplates.StandardTransfer(
                faucetAcct.PublicKey,
                myAccountPK,
                2500_000_000_000,
                100_000_000,
                chainName);
            deploy.Sign(faucetAcct);

            var putResponse = await casperSdk.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Console.WriteLine("Deploy COST : " + execResult.Cost);
        }

        public static async Task DeployContract(string wasmFile)
        {
            var wasmBytes = await File.ReadAllBytesAsync(wasmFile);

            var deploy = DeployTemplates.ContractDeploy(
                wasmBytes,
                myAccountPK,
                50_000_000_000,
                chainName);
            deploy.Sign(myAccount);

            var putResponse = await casperSdk.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();

            Console.WriteLine("Deploy COST : " + execResult.Cost);
        }

        public static async Task GetAccountInfo()
        {
            var response = await casperSdk.GetAccountInfo(myAccountPK);
        }

        public static async Task QueryState()
        {
            var accountKey = new AccountHashKey(myAccountPK);
            var rpcResponse = await casperSdk.QueryGlobalState(accountKey, null,
                "counter/count");

            var result = rpcResponse.Parse();
            Console.WriteLine("Counter value: " + (int) result.StoredValue.CLValue);
        }

        public static async Task QueryStateWithContractHash()
        {
            var response = await casperSdk.GetAccountInfo(myAccountPK);
            var result = response.Parse();
            var hash = result.Account.NamedKeys.First(k => k.Name == "counter").Key;

            var response2 = await casperSdk.QueryGlobalState(hash, null,
                "count");
            
            var gsResult = response2.Parse();
            Console.WriteLine("Counter value: " + gsResult.StoredValue.CLValue.ToInt32());
        }

        public static async Task CallCounterInc()
        {
            var deploy = DeployTemplates.ContractCall(
                "counter",
                "counter_inc",
                null,
                myAccountPK,
                15_000_000,
                chainName);
            deploy.Sign(myAccount);

            var putResponse = await casperSdk.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();

            Console.WriteLine("Deploy COST : " + execResult.Cost);
        }

        public async static Task Main(string[] args)
        {
            // use a logginghandler to print out all communication with the node
            //
            var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
            {
                LoggerStream = new StreamWriter(Console.OpenStandardOutput())
            };
            casperSdk = new NetCasperClient(nodeAddress, loggingHandler);

            try
            {
                await FundAccount();
                Console.WriteLine("FundAccount() completed. Press a key to continue...");
                Console.ReadLine();
                
                await DeployContract("./counter-define.wasm");
                Console.WriteLine("DeployContract() completed. Press a key to continue...");
                Console.ReadLine();

                await GetAccountInfo();
                Console.WriteLine("GetAccountInfo() completed. Press a key to continue...");
                Console.ReadLine();
                
                await QueryState();
                Console.WriteLine("QueryState() completed. Press a key to continue...");
                Console.ReadLine();
                
                await CallCounterInc();
                Console.WriteLine("CallCounterInc() completed. Press a key to continue...");
                Console.ReadLine();
                
                await QueryStateWithContractHash();
                Console.WriteLine("QueryStateWithContractHash() completed. Press a key to continue...");
                Console.ReadLine();
                
                await DeployContract(
                    "./counter_call.wasm");
                Console.WriteLine("DeployContract() completed. Press a key to continue...");
                Console.ReadLine();
                
                await QueryState();
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
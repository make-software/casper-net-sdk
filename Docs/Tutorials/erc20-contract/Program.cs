using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Tutorials
{
    public static class Erc20Contract
    {
        static string nodeAddress = "http://207.154.217.88:11101/rpc";
        static string chainName = "casper-net-1";

        static NetCasperClient casperSdk;

        public static async Task FundAccountWithFaucet(KeyPair faucetAccount, PublicKey myAccountPK, ulong amount)
        {
            var deploy = DeployTemplates.StandardTransfer(
                faucetAccount.PublicKey,
                myAccountPK,
                amount,
                100_000_000,
                chainName);
            deploy.Sign(faucetAccount);

            await casperSdk.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

            var result = deployResponse.Parse();
            var execResult = result.ExecutionResults.First();
            Console.WriteLine("Deploy COST : " + execResult.Cost);
            
            File.WriteAllText("res_FundMyAccount.json", deployResponse.Result.GetRawText());
        }

        public static async Task<HashKey> DeployERC20Contract(KeyPair accountKey)
        {
            var wasmFile = "./erc20_token.wasm";
            var wasmBytes = System.IO.File.ReadAllBytes(wasmFile);

            var header = new DeployHeader()
            {
                Account = accountKey.PublicKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = 1800000,
                ChainName = chainName,
                GasPrice = 1
            };
            var payment = new ModuleBytesDeployItem(300_000_000_000);

            List<NamedArg> runtimeArgs = new List<NamedArg>();
            runtimeArgs.Add(new NamedArg("name", "C# SDK Token"));
            runtimeArgs.Add(new NamedArg("symbol", "CSSDK"));
            runtimeArgs.Add(new NamedArg("decimals", (byte) 5)); //u8
            runtimeArgs.Add(new NamedArg("total_supply", CLValue.U256(10_000)));

            var session = new ModuleBytesDeployItem(wasmBytes, runtimeArgs);

            var deploy = new Deploy(header, payment, session);
            deploy.Sign(accountKey);

            await casperSdk.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

            var execResult = deployResponse.Parse().ExecutionResults.First();

            Console.WriteLine("Deploy COST : " + execResult.Cost);

            var contractHash = execResult.Effect.Transforms.First(t =>
                t.Type == TransformType.WriteContract).Key;
            Console.WriteLine("Contract key: " + contractHash);

            File.WriteAllText("res_DeployERC20Contract.json", deployResponse.Result.GetRawText());

            return (HashKey) contractHash;
        }

        public static async Task GetAccountInfo(PublicKey publicKey)
        {
            var response = await casperSdk.GetAccountInfo(publicKey);

            File.WriteAllText("res_GetAccountInfo.json", response.Result.GetRawText());
        }
        
        public static async Task ReadBalance(string contractHash, PublicKey publicKey)
        {
            var accountHash = new AccountHashKey(publicKey);
            var dictItem = Convert.ToBase64String(accountHash.GetBytes());
            
            var response = await casperSdk.GetDictionaryItemByContract(contractHash, "balances", dictItem);

            File.WriteAllText("res_ReadBalance.json", response.Result.GetRawText());

            var result = response.Parse();
            var balance = result.StoredValue.CLValue.ToBigInteger();
            Console.WriteLine("Balance: " + balance.ToString() + " $CSSDK");
        }

        public static async Task ReadAllowance(string contractHash, PublicKey ownerPk, PublicKey spenderPk)
        {
            var ownerAccHash = new AccountHashKey(ownerPk);
            var spenderAccHash = new AccountHashKey(spenderPk);
            var bytes = new byte[ownerAccHash.GetBytes().Length + spenderAccHash.GetBytes().Length];
            Array.Copy(ownerAccHash.GetBytes(), 0, bytes, 0, ownerAccHash.GetBytes().Length);
            Array.Copy(spenderAccHash.GetBytes(), 0, bytes, ownerAccHash.GetBytes().Length,
                spenderAccHash.GetBytes().Length);

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            bcBl2bdigest.BlockUpdate(bytes, 0, bytes.Length);
            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            var dictItem = Hex.ToHexString(hash);

            try
            {
                var response = await casperSdk.GetDictionaryItemByContract(contractHash, "allowances", dictItem);

                File.WriteAllText("res_ReadAllowance.json", response.Result.GetRawText());

                var result = response.Parse();
                var balance = result.StoredValue.CLValue.ToBigInteger();
                Console.WriteLine("Allowance: " + balance.ToString() + " $CSSDK");
            }
            catch (RpcClientException e)
            {
                if (e.RpcError.Code == -32003)
                    Console.WriteLine("Allowance not found!");
                else
                    throw;
            }
        }
        
        public static async Task TransferTokens(string contractHash, KeyPair ownerAccount, PublicKey recipientPk,
            ulong amount)
        {
            var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
                "transfer",
                new List<NamedArg>()
                {
                    new NamedArg("recipient", CLValue.Key(new AccountHashKey(recipientPk))),
                    new NamedArg("amount", CLValue.U256(amount))
                },
                ownerAccount.PublicKey,
                1_000_000_000,
                chainName);
            deploy.Sign(ownerAccount);

            await casperSdk.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

            File.WriteAllText("res_TransferTokens.json", deployResponse.Result.GetRawText());
        }

        public async static Task ApproveSpender(string contractHash, KeyPair ownerAccount, PublicKey spenderPk, ulong amount)
        {
            var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
                "approve",
                new List<NamedArg>()
                {
                    new NamedArg("spender", CLValue.Key(new AccountHashKey(spenderPk))),
                    new NamedArg("amount", CLValue.U256(amount))
                },
                ownerAccount.PublicKey,
                1_000_000_000,
                chainName);
            deploy.Sign(ownerAccount);

            await casperSdk.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

            File.WriteAllText("res_ApproveSpender.json", deployResponse.Result.GetRawText());
        }

        public static async Task TransferTokensFromOwner(string contractHash, KeyPair spenderAccount, 
            PublicKey ownerPk, PublicKey recipientPk, ulong amount)
        {
            var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
                "transfer_from",
                new List<NamedArg>()
                {
                    new NamedArg("owner", CLValue.Key(new AccountHashKey(ownerPk))),
                    new NamedArg("recipient", CLValue.Key(new AccountHashKey(recipientPk))),
                    new NamedArg("amount", CLValue.U256(amount))
                },
                spenderAccount.PublicKey,
                1_000_000_000,
                chainName);
            deploy.Sign(spenderAccount);

            await casperSdk.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

            File.WriteAllText("res_TransferFromOwner.json", deployResponse.Result.GetRawText());
        }

        public static async Task QueryGlobalState(string key)
        {
            var response = await casperSdk.QueryGlobalState(key);
            
            File.WriteAllText("res_QueryGlobalState.json", response.Result.GetRawText());
        }

        public async static Task Main(string[] args)
        {
            // faucet account in the local network.
            // used to send funds to the erc20 contract owner
            //
            var faucetAcct = KeyPair.FromPem("./faucetact.pem");
            
            // account key of the erc20 contract owner 
            //
            var myAccount = KeyPair.FromPem("./myaccount.pem");
            
            // Alice and Bob account keys
            //
            var aliceAccount = KeyPair.FromPem("./alice_account.pem");
            var bobAccountPK = PublicKey.FromPem("./bob_account_pk.pem");

            // casper client instance with logging enabled
            //
            var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
            {
                LoggerStream = new StreamWriter(Console.OpenStandardOutput())
            };
            casperSdk = new NetCasperClient(nodeAddress, loggingHandler);

            try
            {
                await FundAccountWithFaucet(faucetAcct, myAccount.PublicKey, 1000_000_000_000);
                await FundAccountWithFaucet(faucetAcct, aliceAccount.PublicKey, 50_000_000_000);
                Console.WriteLine("FundAccountWithFaucet() completed. Press a key to continue...");
                Console.ReadLine();

                var contractHash = await DeployERC20Contract(myAccount);
                Console.WriteLine("DeployContract() completed. Press a key to continue...");
                Console.ReadLine();
                
                await QueryGlobalState(contractHash.ToString());
                Console.WriteLine("QueryGlobalState() completed. Press a key to continue...");
                Console.ReadLine();
                
                await GetAccountInfo(myAccount.PublicKey);
                Console.WriteLine("GetAccountInfo() completed. Press a key to continue...");
                Console.ReadLine();
                
                await ReadBalance(contractHash.ToString(), myAccount.PublicKey);
                Console.WriteLine("ReadBalance() completed. Press a key to continue...");
                Console.ReadLine();
                
                await TransferTokens(contractHash.ToString(), myAccount, aliceAccount.PublicKey, 100);
                Console.WriteLine("TransferTokens() completed. Press a key to continue...");
                Console.ReadLine();
                
                await ReadBalance(contractHash.ToString(), aliceAccount.PublicKey);
                Console.WriteLine("ReadBalance() completed. Press a key to continue...");
                Console.ReadLine();
                
                await ApproveSpender(contractHash.ToString(), myAccount, aliceAccount.PublicKey, 1000);
                Console.WriteLine("ApproveSpender() completed. Press a key to continue...");
                Console.ReadLine();

                await ReadAllowance(contractHash.ToString(), myAccount.PublicKey, aliceAccount.PublicKey);
                Console.WriteLine("ReadAllowance() completed. Press a key to continue...");
                Console.ReadLine();
            
                await TransferTokensFromOwner(contractHash.ToString(), aliceAccount, myAccount.PublicKey, bobAccountPK, 75);
                Console.WriteLine("TransferTokensFromOwner() completed. Press a key to continue...");
                Console.ReadLine();
                
                await ReadBalance(contractHash.ToString(), bobAccountPK);
                Console.WriteLine("GetAccountInfo() completed. Press a key to continue...");
                Console.ReadLine();
                
                await ReadAllowance(contractHash.ToString(), myAccount.PublicKey, aliceAccount.PublicKey);
                Console.WriteLine("ReadAllowance() completed. Press a key to continue...");
                Console.ReadLine();
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
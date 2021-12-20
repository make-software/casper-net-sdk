using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Tutorials
{
    public static class KeyValueStorageContract
    {
        static string nodeAddress = "http://207.154.217.11:11101/rpc";
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
                25000_000_000_000,
                100_000_000,
                chainName);
            deploy.Sign(faucetAcct);

            var response = await casperSdk.PutDeploy(deploy);

            string deployHash = response.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);
            File.WriteAllText($"res3.json", getResponse.Result.GetRawText());

            var result = getResponse.Parse();
            var execResult = result.ExecutionResults.First();
            Console.WriteLine("Deploy COST : " + execResult.Cost);
        }

        public static async Task<HashKey> DeployKVStorageContract()
        {
            var wasmFile = "./contract.wasm";
            var wasmBytes = System.IO.File.ReadAllBytes(wasmFile);

            var deploy = DeployTemplates.ContractDeploy(
                wasmBytes,
                myAccountPK,
                300_000_000_000,
                chainName);
            deploy.Sign(myAccount);

            var response = await casperSdk.PutDeploy(deploy);

            string deployHash = response.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            var execResult = deployResponse.Parse().ExecutionResults.First();

            Console.WriteLine("Deploy COST : " + execResult.Cost);

            var contractHash = execResult.Effect.Transforms.First(t =>
                t.Type == TransformType.WriteContract).Key;
            Console.WriteLine("Contract key: " + contractHash);

            return (HashKey)contractHash;
        }

        public async static Task GetAccountInfo()
        {
            var response = await casperSdk.GetAccountInfo(myAccountPK);

            File.WriteAllText("res_GetAccountInfo.json", response.Result.GetRawText());
        }

        public async static Task StoreBool()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "IsSunday"),
                new NamedArg("value", true)
            };
                
            await StoreKeyValue("store_bool", namedArgs, "res_StoreBool.json");
        }

        public async static Task StoreI32()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "I32MinValue"),
                new NamedArg("value", int.MinValue)
            };

            await StoreKeyValue("store_i32", namedArgs, "res_StoreI32.json");
        }

        public async static Task StoreU64()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "U64MaxValue"),
                new NamedArg("value", ulong.MaxValue)
            };

            await StoreKeyValue("store_u64", namedArgs, "res_StoreU64.json");
        }

        public async static Task StoreU512()
        {
            var value = new BigInteger(5_123_456_789_012);
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyU512"),
                new NamedArg("value", CLValue.U512(value))
            };

            await StoreKeyValue("store_u512", namedArgs, "res_StoreU512.json");
        }

        public async static Task StoreString(HashKey contractHash = null)
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "WorkingOn"),
                new NamedArg("value", "Casper .NET SDK")
            };

            var deploy = DeployTemplates.ContractCall(contractHash,
                "store_string",
                namedArgs,
                myAccountPK,
                500_000_000,
                chainName);

            deploy.Sign(myAccount);

            var response = await casperSdk.PutDeploy(deploy);
            var deployHash = response.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            File.WriteAllText("res_StoreString.json", deployResponse.Result.GetRawText());
        }

        public async static Task StorePublicKey()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyPublicKey"),
                new NamedArg("value", myAccountPK)
            };

            await StoreKeyValue("store_public_key", namedArgs, "res_StorePublicKey.json");
        }

        public async static Task StoreGlobalStateKey()
        {
            var gsKey = GlobalStateKey.FromString(
                "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7");

            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyGSKey"),
                new NamedArg("value", gsKey)
            };

            await StoreKeyValue("store_key", namedArgs, "res_StoreGlobalStateKey.json");
        }

        public async static Task StoreOption()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyOption"),
                new NamedArg("value", CLValue.Option(CLValue.String("Optional string")))
            };

            await StoreKeyValue("store_option", namedArgs, "res_StoreOption.json");
        }

        public async static Task StoreByteArray()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyByteArray"),
                new NamedArg("value", Hex.Decode("0F0E0D"))
            };

            await StoreKeyValue("store_byte_array", namedArgs, "res_StoreByteArray.json");
        }

        public async static Task StoreResultOk()
        {
            var result = CLValue.Ok("Success", CLType.String);
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyResultOk"),
                new NamedArg("value", result)
            };

            await StoreKeyValue("store_result", namedArgs, "res_StoreResultOk.json");
        }

        public async static Task StoreResultErr()
        {
            var result = CLValue.Err("Failure", CLType.String);
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyResultErr"),
                new NamedArg("value", result)
            };

            await StoreKeyValue("store_result", namedArgs, "res_StoreResultErr.json");
        }

        public async static Task StoreListOfBytes()
        {
            var list = CLValue.List(new[]
                {CLValue.U8(0x10), CLValue.U8(0x20), CLValue.U8(0x30), CLValue.U8(0x40)});
            
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyListOfBytes"),
                new NamedArg("value", list)
            };

            await StoreKeyValue("store_list_of_bytes", namedArgs, "res_StoreListOfBytes.json");
        }

        public async static Task StoreListOfOptions()
        {
            var list = CLValue.List(new[]
            {
                CLValue.Option("String1"),
                CLValue.Option("String2"),
                CLValue.OptionNone(CLType.String),
                CLValue.Option("String4"),
            });
            
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyListOption"),
                new NamedArg("value", list)
            };

            await StoreKeyValue("store_list_of_options", namedArgs, "res_StoreListOfOptions.json");
        }

        public async static Task StoreListOfPublicKeys()
        {
            var pk1 = PublicKey.FromHexString("0111bc2070A9Af0F26F94B8549BfFa5643eAd0bc68EBA3b1833039cFa2A9a8205d");
            var pk2 = PublicKey.FromHexString("02029d865f743f9a67c82c84d443cbd8187bc4a08ca7b4c985f0caca1a4ee98b1f4c");
            var pk3 = PublicKey.FromHexString("01b92e36567350dd7b339d709bfe341df6fda853e85315418f1bb3ddd414d9f5be");

            var list = CLValue.List(new[]
                {CLValue.PublicKey(pk1), CLValue.PublicKey(pk2), CLValue.PublicKey(pk3)});
            
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyListOfPKs"),
                new NamedArg("value", list)
            };

            await StoreKeyValue("store_list_of_public_key", namedArgs, "res_StoreListOfBytes.json");
        }

        public async static Task StoreMap()
        {
            var dict = new Dictionary<CLValue, CLValue>()
            {
                {CLValue.String("fourteen"), CLValue.Option(CLValue.String("14"))},
                {CLValue.String("fifteen"), CLValue.Option(CLValue.String("15"))},
                {CLValue.String("sixteen"), CLValue.Option(CLValue.String("16"))},
                {CLValue.String("none"), CLValue.OptionNone(CLType.String)},
            };

            var map = CLValue.Map(dict);
            
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyMap"),
                new NamedArg("value", map)
            };

            await StoreKeyValue("store_map", namedArgs, "res_StoreMap.json");
        }

        public async static Task StoreTuple1()
        {
            var tuple = CLValue.Tuple1(CLValue.Option("hola!"));
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyTuple1"),
                new NamedArg("value", tuple)
            };

            await StoreKeyValue("store_tuple1", namedArgs, "res_StoreTuple1.json");
        }

        public async static Task StoreTuple2()
        {
            var tuple = CLValue.Tuple2(CLValue.String("hola"), CLValue.U512(new BigInteger(1024)));
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyTuple2"),
                new NamedArg("value", tuple)
            };

            await StoreKeyValue("store_tuple2", namedArgs, "res_StoreTuple2.json");
        }

        public async static Task StoreTuple3()
        {
            var tuple = CLValue.Tuple3(CLValue.PublicKey(myAccountPK),
                CLValue.Option(CLValue.String("hola!")),
                CLValue.U512(2048));
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "MyTuple3"),
                new NamedArg("value", tuple)
            };

            await StoreKeyValue("store_tuple3", namedArgs, "res_StoreTuple3.json");
        }

        private async static Task StoreKeyValue(string entryPoint, List<NamedArg> namedArgs, string saveToFile)
        {
            var deploy = DeployTemplates.ContractCall("kvstorage_contract",
                entryPoint,
                namedArgs,
                myAccountPK,
                1_000_000_000,
                chainName);
            deploy.Sign(myAccount);

            var response = await casperSdk.PutDeploy(deploy);
            var deployHash = response.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

            if (saveToFile != null)
                File.WriteAllText(saveToFile, deployResponse.Result.GetRawText());
        }

        public async static Task<CLValue> ReadStoredValue(string namedKey)
        {
            var accountKey = new AccountHashKey(myAccountPK);
            var rpcResponse = await casperSdk.QueryGlobalState(accountKey, null,
                $"kvstorage_contract/{namedKey}");

            Console.WriteLine(rpcResponse.Result.GetRawText());
            File.WriteAllText($"res_ReadStoredValue_{namedKey}.json", rpcResponse.Result.GetRawText());

            return rpcResponse.Parse().StoredValue.CLValue;
        }

        public async static Task<CLValue> ReadStoredValue(string namedKey, GlobalStateKey contractHash)
        {
            var queryResponse = await casperSdk.QueryGlobalState(contractHash, null,
                namedKey);

            Console.WriteLine(queryResponse.Result.GetRawText());
            File.WriteAllText($"res_ReadStoredValue_{namedKey}.json", queryResponse.Result.GetRawText());

            return queryResponse.Parse().StoredValue.CLValue;
        }

        public async static Task Main(string[] args)
        {
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
                
                var contractHash = await DeployKVStorageContract();
                Console.WriteLine("DeployContract() completed. Press a key to continue...");
                Console.ReadLine();
                
                await GetAccountInfo();
                Console.WriteLine("GetAccountInfo() completed. Press a key to continue...");
                Console.ReadLine();
                
                await StoreI32();
                Console.WriteLine("StoreI32() completed. Press a key to continue...");
                Console.ReadLine();
                
                var clIntValue = await ReadStoredValue("I32MinValue");
                var number = clIntValue.ToInt32();
                Console.WriteLine("I32MinValue value: " + number);
                Console.WriteLine("ReadStoredValue() completed. Press a key to continue...");
                Console.ReadLine();

                await StoreString(contractHash);
                Console.WriteLine("StoreString() completed. Press a key to continue...");
                Console.ReadLine();
                
                var clValue = await ReadStoredValue("WorkingOn", contractHash);
                Console.WriteLine("WorkingOn Value: " + (string)clValue);
                Console.WriteLine("ReadStoredValue() completed. Press a key to finish...");
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
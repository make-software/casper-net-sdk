using System.IO;
using System.Numerics;
using System.Text.Json;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class RpcResultTest
    {
        [Test]
        public void AccountDeserializerTest()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/account.json");
            var account = JsonSerializer.Deserialize<Account>(json);
            Assert.IsNotNull(account);
            Assert.AreEqual("account-hash-0102030401020304010203040102030401020304010203040102030401020304",
                account.AccountHash.ToString());
            Assert.AreEqual(2, account.NamedKeys.Count);
            Assert.AreEqual("named_key_2", account.NamedKeys[1].Name);
            Assert.AreEqual("uref-0102030401020304010203040102030401020304010203040102030401020304-007",
                account.NamedKeys[1].Key.ToString());
            Assert.AreEqual("uref-0102030401020304010203040102030401020304010203040102030401020304-007",
                account.MainPurse.ToString());
            Assert.AreEqual(1, account.AssociatedKeys.Count);
            Assert.AreEqual("account-hash-0102030401020304010203040102030401020304010203040102030401020304",
                account.AssociatedKeys[0].AccountHash.ToString());
            Assert.AreEqual(1, account.AssociatedKeys[0].Weight);
            Assert.AreEqual(2, account.ActionThresholds.Deployment);
            Assert.AreEqual(3, account.ActionThresholds.KeyManagement);
        }

        [Test]
        public void SuccessExecutionResultDeserializerTest()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/execution-results-success.json");

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters = {new ExecutionResult.ExecutionResultConverter()}
            };
            var results = JsonSerializer.Deserialize<ExecutionResult>(json, options);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.IsSuccess);
            Assert.IsNull(results.ErrorMessage);
            Assert.AreEqual(System.Numerics.BigInteger.Parse("42905260"), results.Cost);
            Assert.AreEqual(0, results.Transfers.Count);

            Assert.AreEqual(10, results.Effect.Operations.Count);
            Assert.AreEqual("hash-efef434ae3cea598a8871ac40ac5d5b00f0f4babe8f7b49a05f87d203d1c8391",
                results.Effect.Operations[0].Key.ToString().ToLower());
            Assert.AreEqual(OpKind.Read, results.Effect.Operations[0].Kind);
            Assert.AreEqual("balance-d86ed76303a691d12cf121e4b4cb4fd875484f28a33c2edf14aad56c01c8c601",
                results.Effect.Operations[9].Key.ToString().ToLower());
            Assert.AreEqual(OpKind.Write, results.Effect.Operations[9].Kind);

            Assert.AreEqual(10, results.Effect.Transforms.Count);
            Assert.AreEqual("hash-98eca31f263ff3176518b6dcbb6af54c7469b60192749feee073d20618a389e6",
                results.Effect.Transforms[0].Key.ToString().ToLower()
            );
            Assert.AreEqual("uref-a3a79be7bd922c0846ed406b01f2430dd6ba367a2703b8d75109a9f41fdc336b-000",
                results.Effect.Transforms[9].Key.ToString().ToLower()
            );
        }

        [Test]
        public void FailureExecutionResultDeserializerTest()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/execution-results-failure.json");

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters = {new ExecutionResult.ExecutionResultConverter()}
            };
            var results = JsonSerializer.Deserialize<ExecutionResult>(json, options);
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsSuccess);
            Assert.IsNotNull(results.ErrorMessage);
            Assert.AreEqual(
                "Wasm preprocessing error: Encountered operation forbidden by gas rules. Consult instruction -> metering config map",
                results.ErrorMessage);
            Assert.AreEqual(BigInteger.Zero, results.Cost);
            Assert.AreEqual(0, results.Transfers.Count);
            Assert.AreEqual(0, results.Effect.Operations.Count);
            Assert.AreEqual(0, results.Effect.Transforms.Count);
        }

        [Test]
        public void RpcResultLoadTest()
        {
            var result = RpcResult.Load<GetAccountInfoResult>(TestContext.CurrentContext.TestDirectory +
                                                              "/TestData/rpc-result.json");
            Assert.IsNotNull(result);
            Assert.AreEqual("1.0.0", result.ApiVersion);
            Assert.AreEqual("account-hash-29e1d9b2fa30d946ffec1a7cc3d2f6852a72227d6dc089edc93bf7c74ad0a444",
                result.Account.AccountHash.ToString().ToLower());
            Assert.AreEqual(3, result.Account.NamedKeys.Count);
        }
        
        [Test]
        public void RpcResultParseTest()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/rpc-result.json");
            
            var result = RpcResult.Parse<GetAccountInfoResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("uref-0d1054710fe5fb8c2ae758996f32df6e5cac2d6bfd1be8c859ab07aaa5128d7b-007",
                result.Account.MainPurse.ToString().ToLower());
            Assert.AreEqual(1, result.Account.AssociatedKeys.Count);
            Assert.AreEqual(result.Account.AccountHash.ToString(), 
                result.Account.AssociatedKeys[0].AccountHash.ToString());
        }
    }
}
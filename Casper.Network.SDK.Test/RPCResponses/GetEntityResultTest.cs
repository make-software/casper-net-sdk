using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class GetEntityResultTest
    {
        [Test]
        public void GetEntityAccountTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-entity-account-v200.json");

            var result = RpcResult.Parse<GetEntityResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            AssertExtensions.IsValidHex(result.MerkleProof);
            
            Assert.IsNotNull(result.Entity.Package);
            Assert.IsNotNull(result.Entity.MainPurse);
            Assert.AreEqual("byte-code-0000000000000000000000000000000000000000000000000000000000000000", result.Entity.ByteCodeHash);
            Assert.IsNotEmpty(result.Entity.EntityKind.Account.ToHexString());
            Assert.AreEqual(1, result.Entity.ActionThresholds.KeyManagement);
            Assert.AreEqual(1, result.Entity.ActionThresholds.Deployment);
            Assert.AreEqual(1, result.Entity.ActionThresholds.UpgradeManagement);
            Assert.AreEqual(0, result.NamedKeys.Count);
            Assert.AreEqual(0, result.EntryPoints.Count);
        }
        
        [Test]
        public void GetEntityContractTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-entity-contract-v200.json");

            var result = RpcResult.Parse<GetEntityResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            AssertExtensions.IsValidHex(result.MerkleProof);
            
            Assert.IsNotNull(result.Entity.Package);
            Assert.IsNotNull(result.Entity.MainPurse);
            Assert.AreEqual("byte-code-85def61e3ee02e10a1e845cfb8e8b2d9640a18f605333158027a24ed8569d895", result.Entity.ByteCodeHash);
            Assert.AreEqual(TransactionRuntime.VmCasperV1, result.Entity.EntityKind.SmartContract);
            Assert.AreEqual(1, result.Entity.ActionThresholds.KeyManagement);
            Assert.AreEqual(1, result.Entity.ActionThresholds.Deployment);
            Assert.AreEqual(1, result.Entity.ActionThresholds.UpgradeManagement);
            Assert.AreEqual(11, result.NamedKeys.Count);
            Assert.AreEqual("balances", result.NamedKeys[1].Name);
            Assert.AreEqual("uref-96cd0453fb2e1d063c9438c158c0d804d0121a96f3423046150ee355cfadefb6-007", result.NamedKeys[1].Key.ToString().ToLower());
            
            Assert.IsTrue(result.EntryPoints.Count > 0);
            Assert.AreEqual("allowance", result.EntryPoints[1].Name);
            Assert.AreEqual(2, result.EntryPoints[1].Args.Count);
            Assert.AreEqual("spender", result.EntryPoints[1].Args[1].Name);
            Assert.AreEqual(CLType.Key, result.EntryPoints[1].Args[1].CLType);
            Assert.AreEqual(CLType.U256, result.EntryPoints[1].Ret);
            Assert.IsTrue(result.EntryPoints[1].Access.IsPublic);
            Assert.AreEqual(EntryPointType.Called, result.EntryPoints[1].EntryPointType);
        }
    }
}
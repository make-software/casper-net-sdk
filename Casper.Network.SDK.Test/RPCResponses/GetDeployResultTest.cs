using System.IO;
using System.Linq;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetDeployResultTest
    {
        [Test]
        public void GetDeployResultTest_Version1_Success()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-deploy-result-version1-success.json");
            
            var result = RpcResult.Parse<GetDeployResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            var deploy = result.Deploy;
            AssertExtensions.IsHash(deploy.Hash);
            AssertExtensions.IsValidHex(deploy.Header.Account.ToAccountHex(), 33);
            Assert.AreEqual(1, deploy.Payment.RuntimeArgs.Count);
            Assert.AreEqual(6, deploy.Session.RuntimeArgs.Count);
            Assert.AreEqual(1, deploy.Approvals.Count);
            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.AreEqual(23, result.ExecutionInfo.BlockHeight);
            Assert.IsTrue(result.ExecutionInfo.ExecutionResult.Effect.Count > 0);
            Assert.IsNull(result.ExecutionInfo.ExecutionResult.ErrorMessage);
        }
        
        [Test]
        public void GetDeployResultTest_Version1_Failure()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-deploy-result-version1-failure.json");
            
            var result = RpcResult.Parse<GetDeployResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            var deploy = result.Deploy;
            AssertExtensions.IsHash(deploy.Hash);
            AssertExtensions.IsValidHex(deploy.Header.Account.ToAccountHex(), 33);
            Assert.AreEqual(1, deploy.Payment.RuntimeArgs.Count);
            Assert.AreEqual(2, deploy.Session.RuntimeArgs.Count);
            Assert.AreEqual(1, deploy.Approvals.Count);
            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.AreEqual(122, result.ExecutionInfo.BlockHeight);
            Assert.IsTrue(result.ExecutionInfo.ExecutionResult.Effect.Count > 0);
            Assert.AreEqual("User error: 60001", result.ExecutionInfo.ExecutionResult.ErrorMessage);
            Assert.AreEqual("31057410", result.ExecutionInfo.ExecutionResult.Cost.ToString());
        }
        
        [Test]
        public void GetDeployResultTest_Version2_Success()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-deploy-result-version2-success.json");
            
            var result = RpcResult.Parse<GetDeployResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            var deploy = result.Deploy;
            AssertExtensions.IsHash(deploy.Hash);
            AssertExtensions.IsValidHex(deploy.Header.Account.ToAccountHex(), 33);
            Assert.AreEqual(1, deploy.Payment.RuntimeArgs.Count);
            Assert.AreEqual(2, deploy.Session.RuntimeArgs.Count);
            Assert.AreEqual(1, deploy.Approvals.Count);
            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.AreEqual(1964, result.ExecutionInfo.BlockHeight);
            Assert.IsTrue(result.ExecutionInfo.ExecutionResult.Effect.Count > 0);
            Assert.IsNull(result.ExecutionInfo.ExecutionResult.ErrorMessage);
        }
    }
}
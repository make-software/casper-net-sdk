using Casper.Network.SDK.JsonRpc;
using NUnit.Framework;

namespace NetCasperTest
{
    public class CasperMethodsTest
    {
        private string dummyDeployHash = "000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f";
        
        [Test]
        public void GetDeployMethodTest()
        {
            var method = new GetDeploy(dummyDeployHash);
            var jsonStr = method.Serialize();
            Assert.IsFalse(jsonStr.Contains("finalized_approvals"));

            method = new GetDeploy(dummyDeployHash, false);
            jsonStr = method.Serialize();
            Assert.IsFalse(jsonStr.Contains("finalized_approvals"));

            method = new GetDeploy(dummyDeployHash, true);
            jsonStr = method.Serialize();
            Assert.IsTrue(jsonStr.Contains(@"""finalized_approvals"": true"));
        }
    }
}

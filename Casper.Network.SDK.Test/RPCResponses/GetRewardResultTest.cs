using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class GetRewardResultTest
    {
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/info-get-reward-v200.json");

            var result = RpcResult.Parse<GetRewardResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual(13, result.EraId);
            Assert.AreEqual(1, result.DelegationRate);
            Assert.AreEqual("62559062048560", result.Amount.ToString());
        }
    }
}
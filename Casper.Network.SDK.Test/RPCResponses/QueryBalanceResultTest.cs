using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class GetBalanceResultTest
    {
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/query-balance-v200.json");

            var result = RpcResult.Parse<QueryBalanceResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual("1576039612769", result.BalanceValue.ToString());
        }
    }
}
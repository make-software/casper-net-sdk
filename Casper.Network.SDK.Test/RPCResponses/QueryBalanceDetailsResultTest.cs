using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class QueryBalanceDetailsResultTest
    {
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/query-balance-details-v200.json");

            var result = RpcResult.Parse<QueryBalanceDetailsResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual(7, result.Holds.Count);
            Assert.IsTrue(result.Holds[1].Amount > 0);            
            Assert.IsTrue(result.Holds[1].Time > 0);            
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Holds[1].Proof));            
            Assert.IsTrue(result.TotalBalance > 0);
            Assert.IsTrue(result.AvailableBalance > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.TotalBalanceProof));            
        }
    }
}
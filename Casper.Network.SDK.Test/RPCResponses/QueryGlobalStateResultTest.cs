using System.IO;
using System.Numerics;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class QueryGlobalStateResultTest
    {
        [Test]
        public void LegacyTransferTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/legacy_transfer_v200.json");

            var result = RpcResult.Parse<QueryGlobalStateResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.IsNotNull(result.StoredValue.Transfer);
            var transfer = result.StoredValue.Transfer;
            Assert.AreEqual(new BigInteger(2500000000000), transfer.Amount);
            Assert.AreEqual("cd91f138e82ddce5dfbb99c6bbf3f47caca439b81d7f43702cbebfa99bacbfd0", transfer.TransactionHash.Deploy);
            Assert.AreEqual("account-hash-87516c22bca9a14179ebbbe646c8f911153fe53626126c1ba24293517c2e04a2", transfer.From.AccountHash.ToString());
            Assert.AreEqual("account-hash-1265caa7cd80f31f882ab6f5623d89741c77cf3a36a309b54ed55fdc0be227c9", transfer.To.ToString());
            Assert.AreEqual("uref-c003c853c6477137225914c7610cf2c30ed594480c1fccd6dfd58a633522edc6-007", transfer.Source.ToString());
            Assert.AreEqual("uref-c74ad99f2bca136c1e64b1a14bf576696363c25701d66d87adca6dbdb147b39d-004", transfer.Target.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.MerkleProof));
        }
    }
}
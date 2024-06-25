using System.IO;
using System.Linq;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetBlockResultTest
    {
        [Test]
        public void GetBlockResultTest_v156()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-block-result-v156.json");
            
            var result = RpcResult.Parse<GetBlockResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("1.5.6", result.ApiVersion);
            Assert.AreEqual(3, result.Proofs.Count);
            Assert.IsNotNull(result.Proofs[1].PublicKey);            
            AssertExtensions.IsValidHex(result.Proofs[1].Signature.ToString(), 65);     
            Assert.AreEqual(1, result.Block.Version);
            AssertExtensions.IsHash(result.Block.Hash);
            Assert.AreEqual(2947381, result.Block.Header.Height);
            Assert.AreEqual(13670, result.Block.Header.EraId);
            Assert.IsNull(result.Block.Header.EraEnd);
            Assert.IsNull(result.Block.Body.RewardedSignatures);
            Assert.AreEqual(26, result.Block.Body.Transactions.Count);
            Assert.AreEqual(25, result.Block.Body.Transactions
                .Where(t => t.Category==TransactionCategory.Large).ToList().Count);
            Assert.AreEqual("31849e17f715273ad7032d51534c5b6029dd0dec1e01c225c50083752a750219",
                result.Block.Body.Transactions.First(t => t.Category==TransactionCategory.Mint).Hash);
        }
        
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-block-result-v200.json");
            
            var result = RpcResult.Parse<GetBlockResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual(5, result.Proofs.Count);
            Assert.IsNotNull(result.Proofs[1].PublicKey);            
            AssertExtensions.IsValidHex(result.Proofs[1].Signature.ToString(), 65);     
            Assert.AreEqual(2, result.Block.Version);
            AssertExtensions.IsHash(result.Block.Hash);
            Assert.AreEqual(1551, result.Block.Header.Height);
            Assert.AreEqual(141, result.Block.Header.EraId);
            Assert.IsNotNull(result.Block.Header.EraEnd);
            var eraEnd = result.Block.Header.EraEnd;
            Assert.IsTrue(eraEnd.NextEraValidatorWeights.Count > 1);
            Assert.IsNotNull(eraEnd.NextEraValidatorWeights[1].PublicKey);
            Assert.IsNotNull(eraEnd.NextEraValidatorWeights[1].Weight);
            Assert.IsTrue(eraEnd.Rewards.Count > 1);
            Assert.AreEqual(1, result.Block.Body.Transactions.Count);
            Assert.AreEqual(TransactionCategory.InstallUpgrade, result.Block.Body.Transactions[0].Category);
            Assert.IsNotEmpty(result.Block.Body.Transactions[0].Hash);
        }
    }
}
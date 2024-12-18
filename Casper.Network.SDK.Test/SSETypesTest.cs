using System.IO;
using System.Text.Json;
using Casper.Network.SDK.SSE;
using NUnit.Framework;

namespace NetCasperTest
{
    public class SSETypesTest
    {
        [Test]
        public void FinalitySignatureV1Test()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/finality_signature_v156.json";
            var json = File.ReadAllText(testFile);

            var doc = System.Text.Json.JsonDocument.Parse(json);

            json = doc.RootElement.GetProperty("FinalitySignature").GetRawText();
            // Assert.AreEqual(3, doc.RootElement.EnumerateObject().Count());
            // Assert.AreEqual("U512", );
            var value = JsonSerializer.Deserialize<FinalitySignature>(json);
            Assert.IsNotNull(value);

            Assert.AreEqual(1, value.Version);
            Assert.AreEqual("d800de72aa40c6df064c714d3a6d8b6fab73f68742d8468b84efd6616bbb10bb", value.BlockHash);
            Assert.AreEqual(0, value.BlockHeight);
            Assert.AreEqual(13859, value.EraId);
            Assert.IsNull(value.ChainNameHash);
            Assert.AreEqual(
                "01221b61b83b889898363501c7defd7baa6729989d8fce2dfffea4632017fd46cc34b88bc85fa570a6e9ada829c67b9fdaa78e8dee2f07d346bcad0010e1d3df0f",
                value.Signature.ToHexString().ToLowerInvariant());
            Assert.AreEqual("01b71b2d746681b4e0f44ef137d72ee0d42122b08ef569dc65bb0395cb624f99e5",
                value.PublicKey.ToString().ToLowerInvariant());

            var v1 = (FinalitySignatureV1)value;

            Assert.AreEqual("d800de72aa40c6df064c714d3a6d8b6fab73f68742d8468b84efd6616bbb10bb", v1.BlockHash);
            Assert.AreEqual(13859, v1.EraId);
            Assert.AreEqual(
                "01221b61b83b889898363501c7defd7baa6729989d8fce2dfffea4632017fd46cc34b88bc85fa570a6e9ada829c67b9fdaa78e8dee2f07d346bcad0010e1d3df0f",
                v1.Signature.ToHexString().ToLowerInvariant());
            Assert.AreEqual("01b71b2d746681b4e0f44ef137d72ee0d42122b08ef569dc65bb0395cb624f99e5",
                v1.PublicKey.ToString().ToLowerInvariant());
        }

        [Test]
        public void FinalitySignatureV2Test()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/finality_signature_v2.json";
            var json = File.ReadAllText(testFile);

            var doc = System.Text.Json.JsonDocument.Parse(json);

            json = doc.RootElement.GetProperty("FinalitySignature").GetRawText();
            // Assert.AreEqual(3, doc.RootElement.EnumerateObject().Count());
            // Assert.AreEqual("U512", );
            var value = JsonSerializer.Deserialize<FinalitySignature>(json);
            Assert.IsNotNull(value);

            Assert.AreEqual(2, value.Version);
            Assert.AreEqual("13a5603c1dad7d4e4d2ce81313c35043172e1535363c3ec428f559dff5704ea5", value.BlockHash);
            Assert.AreEqual(54, value.BlockHeight);
            Assert.AreEqual(5, value.EraId);
            Assert.AreEqual("8a09603fc862b15412b60a050d71f69c57601b6da7382dd56b9a3f300822bb75", value.ChainNameHash);
            Assert.AreEqual(
                "01a12e3601b4d5c82177231910adaabf50d8a52416e756efee1694ee7534ae16fdb59a7370d564715615736074850b9255ee246c8ffa2502c167c6ef8a86b06504",
                value.Signature.ToHexString().ToLowerInvariant());
            Assert.AreEqual("01fed662dc7f1f7af43ad785ba07a8cc05b7a96f9ee69613cfde43bc56bec1140b",
                value.PublicKey.ToString().ToLowerInvariant());
        }

        [Test]
        public void TransactionProcessedTest()
        {
            string testFile = TestContext.CurrentContext.TestDirectory +
                              "/TestData/sse-transaction-processed-v200.json";
            var json = File.ReadAllText(testFile);

            var value = JsonSerializer.Deserialize<TransactionProcessed>(json);
            Assert.IsNotNull(value);

            Assert.AreEqual("9749ce1dc5dbd0d9a611088f934fd81c2c8429dbab0a3a7d281359be0a92d29a",
                value.TransactionHash.Version1);
            Assert.AreEqual("01a5a5b7328118681638be3e06c8749609280dba4c9daf9aeb3d3464b8839b018a",
                value.InitiatorAddr.PublicKey.ToString().ToLower());
            Assert.AreEqual("0dabde3e8b065e734247b7d5328ac18317af9842f0141ffe41173df15efd97a8", value.BlockHash);
            Assert.IsTrue(value.ExecutionResult.Effect.Count > 0);

            Assert.AreEqual(1, value.Messages.Count);
            var message = value.Messages[0];
            Assert.AreEqual("9038763d3066f0217047263ebd48dc7c839fadfdde141f5b990866563655b44a",
                message.HashAddr);
            Assert.AreEqual("dummy-data", message.MessagePayload.String);
            Assert.IsNull(message.MessagePayload.Bytes);
            Assert.AreEqual("events", message.TopicName);
            Assert.AreEqual("5721a6d9d7a9afe5dfdb35276fb823bed0f825350e4d865a5ec0110c380de4e1", message.TopicNameHash);
            Assert.AreEqual(1, message.TopicIndex);
            Assert.AreEqual(2, message.BlockIndex);
        }
    }
}
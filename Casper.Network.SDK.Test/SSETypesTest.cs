using System.IO;
using System.Text.Json;
using Casper.Network.SDK.SSE;
using NUnit.Framework;

namespace NetCasperTest
{
    public class SSETypesTest
    {
        [Test]
        public void FinalitySignatureV2Test()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/finality_signature_v2.json";
            var json = File.ReadAllText(testFile);
            
            var doc = System.Text.Json.JsonDocument.Parse(json);

            json = doc.RootElement.GetProperty("FinalitySignature").GetRawText();
            // Assert.AreEqual(3, doc.RootElement.EnumerateObject().Count());
            // Assert.AreEqual("U512", );
            var value = JsonSerializer.Deserialize<IFinalitySignature>(json);
            Assert.IsNotNull(value);

            var v2 = value.FinalitySignatureV2;
            Assert.IsNotNull(v2);
            Assert.AreEqual("13a5603c1dad7d4e4d2ce81313c35043172e1535363c3ec428f559dff5704ea5", v2.BlockHash);
            Assert.AreEqual(54, v2.BlockHeight);
            Assert.AreEqual(5, v2.EraId);
            Assert.AreEqual("8a09603fc862b15412b60a050d71f69c57601b6da7382dd56b9a3f300822bb75", v2.ChainNameHash);
            Assert.AreEqual("01a12e3601b4d5c82177231910adaabf50d8a52416e756efee1694ee7534ae16fdb59a7370d564715615736074850b9255ee246c8ffa2502c167c6ef8a86b06504", 
                v2.Signature.ToHexString().ToLowerInvariant());
            Assert.AreEqual("01fed662dc7f1f7af43ad785ba07a8cc05b7a96f9ee69613cfde43bc56bec1140b", 
                v2.PublicKey.ToString().ToLowerInvariant());
            
        }
    }
}
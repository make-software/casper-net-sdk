using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetNodeStatusResultTest
    {
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/info-get-status-v200.json");

            var result = RpcResult.Parse<GetNodeStatusResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual(4, result.Peers.Count);
            Assert.IsNotNull(result.Peers[1].NodeId);            
            Assert.IsNotNull(result.Peers[1].Address);   
            Assert.IsTrue(result.BuildVersion.StartsWith("2.0.0-"));
            Assert.AreEqual("2.0.0-f803ee53d", result.BuildVersion);
            Assert.AreEqual("casper-net-1", result.ChainspecName);
            Assert.AreEqual("7ca5855e14e936019193d07055fb3390e21ec8cb75724ea756b4eac84e1b5fd7", result.StartingStateRootHash.ToLower());
            Assert.AreEqual("01fed662dc7f1f7af43ad785ba07a8cc05b7a96f9ee69613cfde43bc56bec1140b", result.OurPublicSigningKey.ToString().ToLower());
            Assert.AreEqual("643f9d06cebd07dffef47b7bfa70cc014496ff904561a78a1b5e8123247d3231", result.LatestSwitchBlockHash.ToLower());
            Assert.AreEqual("89ceb389e0e8244d8f82afacedaccb42d256831c3a036b06c0770323905d97d1", result.LastAddedBlockInfo.Hash.ToLower());
        }
    }
}
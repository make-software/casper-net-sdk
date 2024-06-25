using System.IO;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetAuctionInfoResultTest
    {
        [Test]
        public void GetBlockResultTest_v156()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-auction-info-v156.json");
            
            var result = RpcResult.Parse<GetAuctionInfoResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("1.5.6", result.ApiVersion);
            Assert.AreEqual("436d8e272b759838e6416d915efb3adc3a102489b8db1c0ce731043116c6f645", result.AuctionState.StateRootHash);
            Assert.AreEqual(3192405, result.AuctionState.BlockHeight);
            Assert.AreEqual(1, result.AuctionState.EraValidators.Count);
            Assert.AreEqual(13801, result.AuctionState.EraValidators[0].EraId);
            Assert.AreEqual(5, result.AuctionState.EraValidators[0].ValidatorWeights.Count);
            Assert.AreEqual("020377bc3ad54b5505971e001044ea822a3f6f307f8dc93fa45a05b7463c0a053bed", result.AuctionState.EraValidators[0].ValidatorWeights[4].PublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("10567495110201092"), result.AuctionState.EraValidators[0].ValidatorWeights[4].Weight);
            
            Assert.AreEqual(3, result.AuctionState.Bids.Count);
            Assert.AreEqual("01001b79b9a6e13d2b96e916f7fa7dff40496ba5188479263ca0fb2ccf8b714305", result.AuctionState.Bids[0].PublicKey.ToString().ToLower());
            Assert.AreEqual(1, result.AuctionState.Bids[0].Delegators.Count);
            Assert.AreEqual("018b34b15e023844531621cb52d42e216a2ea56034f0f40bf7cee566c32eae4f83", result.AuctionState.Bids[0].Delegators[0].DelegatorPublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("30268476029"), result.AuctionState.Bids[0].Delegators[0].StakedAmount);
            Assert.IsNull(result.AuctionState.Bids[0].Delegators[0].VestingSchedule);
        }
        
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-auction-info-v200.json");
            
            var result = RpcResult.Parse<GetAuctionInfoResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual("1e8f22fb799932c56ffcf4d48c014e09ba9b791a3280f9a8cc9c7614ce7d562e", result.AuctionState.StateRootHash);
            Assert.AreEqual(1394, result.AuctionState.BlockHeight);
            Assert.AreEqual(1394, result.AuctionState.BlockHeight);
            Assert.AreEqual(3, result.AuctionState.EraValidators.Count);
            Assert.AreEqual(128, result.AuctionState.EraValidators[2].EraId);
            Assert.AreEqual(5, result.AuctionState.EraValidators[2].ValidatorWeights.Count);
            Assert.AreEqual("01fed662dc7f1f7af43ad785ba07a8cc05b7a96f9ee69613cfde43bc56bec1140b", result.AuctionState.EraValidators[2].ValidatorWeights[4].PublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("1366181433007372460"), result.AuctionState.EraValidators[2].ValidatorWeights[4].Weight);
            
            Assert.AreEqual(5, result.AuctionState.Bids.Count);
            Assert.AreEqual("01fed662dc7f1f7af43ad785ba07a8cc05b7a96f9ee69613cfde43bc56bec1140b", result.AuctionState.Bids[4].PublicKey.ToString().ToLower());
            Assert.AreEqual(3, result.AuctionState.Bids[4].Delegators.Count);
            Assert.AreEqual("0184f6d260f4ee6869ddb36affe15456de6ae045278fa2f467bb677561ce0dad55", result.AuctionState.Bids[4].Delegators[1].DelegatorPublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("654063155243124749"), result.AuctionState.Bids[4].Delegators[1].StakedAmount);
            Assert.AreEqual(1719301233872, result.AuctionState.Bids[4].Delegators[1].VestingSchedule.InitialReleaseTimestampMillis);
        }
    }
}
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    [TestFixture]
    public class CLValueDictionaryTest
    {
        private const string TestHex =
            "eb000000e70000000a0000006576656e745f4d696e74003ead5986ccbcb5b632489b1b60577f1aefd833c0e1ee7ebd3d2a31ff74cb7f320400000031393735ac0000007b226e616d65223a22435350522e6c6976652b2032303234222c226465736372697074696f6e223a224772616e74732061636365737320746f207072656d69756d20666561747572657320696e20435350522e6c697665222c22696d616765223a2268747470733a2f2f6361737065722d6173736574732e73332e616d617a6f6e6177732e636f6d2f637370726c697665706c75732f323032342e706e67222c2279656172223a323032347d0e0320000000cab34dbd7203ecfe5b215545a3854c4b146af40bd0b556019c26c2ac4af4c87c0400000031393735";

        [Test]
        public void Parse_ReturnsCorrectValue()
        {
            var result = CLValueDictionary.Parse(Hex.Decode(TestHex));

            Assert.IsNotNull(result.Value);
        }

        [Test]
        public void Parse_ReturnsCorrectSeed()
        {
            var result = CLValueDictionary.Parse(Hex.Decode(TestHex));

            Assert.AreEqual(
                "uref-cab34dbd7203ecfe5b215545a3854c4b146af40bd0b556019c26c2ac4af4c87c-000",
                result.Seed.ToString());
        }

        [Test]
        public void Parse_ReturnsCorrectItemKey()
        {
            var result = CLValueDictionary.Parse(Hex.Decode(TestHex));

            Assert.AreEqual("1975", result.ItemKey);
        }
    }
}


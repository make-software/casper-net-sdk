using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public static class AssertExtensions
    {
        public static void IsHash(string maybeHash)
        {
            try
            {
                Assert.AreEqual(32, Hex.Decode(maybeHash).Length);
            }
            catch
            {
                Assert.Fail($"Cannot decode a hash from '{maybeHash}'");
            }
        }

        public static void IsValidHex(string maybeHex, uint bytesLength = 0)
        {
            try
            {
                var bytes = Hex.Decode(maybeHex);
                if(bytesLength > 0)
                    Assert.AreEqual(bytesLength, bytes.Length);
            }
            catch
            {
                Assert.Fail($"Cannot decode hex bytes from '{maybeHex}'");
            }
        }
    }
}
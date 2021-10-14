using NetCasperSDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class KeysTest
    {
        [Test]
        public void TestVerifySignatureEd25519()
        {
            var signer = "01b7c7c545dfa3fb853a97fb3581ce10eb4f67a5861abed6e70e5e3312fdde402c";
            var signature =
                "ff70e0fd0653d4cc6c7e67b14c0872db3f74eec6f50d409a7e9129c577237751a1f924680e48cd87a27999c08f422a003867fae09f95f36012289f7bfb7f6f0b";
            var hash = "ef91b6cef0e94a7ab2ffeb896b8266b01ab8003a578f4744d4ee64718771d8da";

            var pk = PublicKey.FromHexString(signer);
            Assert.IsTrue(pk.VerifySignature(Hex.Decode(hash), Hex.Decode(signature)));
        }
        
        [Test]
        public void TestVerifySignatureSECP256K1()
        {
            var signer = "02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e";
            var signature =
                "f03831c61d147204c4456f9b47c3561a8b83496b760a040c901506ec54c54ab357a009d5d4d0b65e40729f7bbbbf042ab8d579d090e7a7aaa98f4aaf2651392e";
            var hash = "d204b74d902e044563764f62e86353923c9328201c82c28fe75bf9fc0c4bbfbc";

            var pk = PublicKey.FromHexString(signer);
            Assert.IsTrue(pk.VerifySignature(Hex.Decode(hash), Hex.Decode(signature)));
        }
    }
}
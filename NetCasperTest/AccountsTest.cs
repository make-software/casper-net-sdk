using NetCasperSDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class AccountsTests
    {
        private static  string ED25519publicKey = "01381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb771";
        private static  string ED25519hash = "07b30fdd279f21d29ab1922313b56ad3905e7dd6a654344b8012e0be9fefa51b";

        private static  string SECP256K1publicKey = "0203b2f8c0613d2d866948c46e296f09faed9b029110d424d19d488a0c39a811ebbc";
        private static  string SECP256K1hash = "aebf6cf44f8d7a633b4e2084ce3be3bbe3db2cec62e49afe103dca79f7818d43";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestValidBlakeEd25519()
        {
            var hash = PublicKey.FromHexString(ED25519publicKey).GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash), ED25519hash, "Unexpected ED25519hash value");
        }

        [Test]
        public void TestValidBlakeSecp256k1()
        {
            var hash = PublicKey.FromHexString(SECP256K1publicKey).GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash), SECP256K1hash, "Unexpected SECP256K1hash value");
        }
    }
}
using System;
using System.Linq;
using System.Net;
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
            var publicKey = PublicKey.FromHexString(ED25519publicKey);
            Assert.AreEqual(KeyAlgo.ED25519, publicKey.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, publicKey.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(publicKey.RawBytes));
            
            var hash = publicKey.GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash), ED25519hash, "Unexpected ED25519 hash value");

            var pk2 = PublicKey.FromBytes(Hex.Decode(ED25519publicKey));
            Assert.AreEqual(KeyAlgo.ED25519, pk2.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, pk2.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(pk2.RawBytes));
            
            var pemfile =  TestContext.CurrentContext.TestDirectory + 
                           "/TestData/test-ed25519-pk.pem";
            var pk3 = PublicKey.FromPem(pemfile);
            Assert.AreEqual(KeyAlgo.ED25519, pk3.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, pk3.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(pk3.RawBytes));
            Assert.IsTrue(Hex.Decode(ED25519publicKey).SequenceEqual(pk3.GetBytes()));
            
            var hash3 = pk3.GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash3), ED25519hash, "Unexpected SECP256K1 hash value");
        }

        [Test]
        public void TestValidBlakeSecp256k1()
        {
            var publicKey = PublicKey.FromHexString(SECP256K1publicKey);
            Assert.AreEqual(KeyAlgo.SECP256K1, publicKey.KeyAlgorithm);
            
            var hash = publicKey.GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash), SECP256K1hash, "Unexpected SECP256K1hash value");
            
            var pk2 = PublicKey.FromBytes(Hex.Decode(SECP256K1publicKey));
            Assert.AreEqual(KeyAlgo.SECP256K1, pk2.KeyAlgorithm);
            Assert.AreEqual(SECP256K1publicKey, pk2.ToAccountHex());
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey)[1..].SequenceEqual(pk2.RawBytes));
            
            var pemfile =  TestContext.CurrentContext.TestDirectory + 
                           "/TestData/test-secp256k1-pk.pem";
            var pk3 = PublicKey.FromPem(pemfile);
            Assert.AreEqual(KeyAlgo.SECP256K1, pk3.KeyAlgorithm);
            Assert.AreEqual(SECP256K1publicKey, pk3.ToAccountHex());
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey)[1..].SequenceEqual(pk3.RawBytes));
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey).SequenceEqual(pk3.GetBytes()));
            
            var hash3 = pk3.GetAccountHash();
            Assert.AreEqual(Hex.ToHexString(hash3), SECP256K1hash, "Unexpected SECP256K1hash value");
        }
        
        [Test]
        public void TestInvalidBlakeEd25519()
        {
            // test that a public key without the algorithm identifier is not valid
            //
            try
            {
                var publicKey = PublicKey.FromHexString(ED25519publicKey.Substring(2));
                Assert.Fail("Exception expected for invalid key");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("Wrong public key algorithm identifier"));
            }
            
            // test that a public key with invalid length fails to create the object
            //
            try
            {
                var publicKey = PublicKey.FromHexString(ED25519publicKey.Substring(0, 64));
                Assert.Fail("Exception expected for invalid key");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("Wrong public key format"));
            }
            
            // test that an empty string fails
            //
            try
            {
                var publicKey = PublicKey.FromHexString("");
                Assert.Fail("Exception expected for invalid key");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("Wrong public key format"));
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class KeysTest
    {
        private static  string ED25519publicKey = "01381b36cd07Ad85348607ffE0fA3A2d033eA941D14763358eBEacE9C8aD3cB771";

        private static  string ED25519hash = "07b30Fdd279f21D29Ab1922313b56ad3905e7dd6A654344B8012e0Be9fefA51B";

        private static  string SECP256K1publicKey = "0203b2F8c0613d2d866948c46e296F09faEd9b029110d424d19d488A0C39a811ebBC";
        private static  string SECP256K1hash = "aeBf6cf44F8d7a633b4E2084CE3bE3BbE3Db2ceC62e49AFe103DCA79f7818D43";

        [Test]
        public void TestValidBlakeEd25519()
        {
            var publicKey = PublicKey.FromHexString(ED25519publicKey);
            Assert.AreEqual(KeyAlgo.ED25519, publicKey.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, publicKey.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(publicKey.RawBytes));
            
            var hash = publicKey.GetAccountHash();
            Assert.AreEqual(ED25519hash, hash.Substring("account-hash-".Length), "Unexpected ED25519 hash value");

            var pk2 = PublicKey.FromBytes(Hex.Decode(ED25519publicKey));
            Assert.AreEqual(KeyAlgo.ED25519, pk2.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, pk2.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(pk2.RawBytes));
            
            var pk3 = PublicKey.FromRawBytes(Hex.Decode(ED25519publicKey)[1..], KeyAlgo.ED25519);
            Assert.AreEqual(KeyAlgo.ED25519, pk3.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, pk3.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(pk3.RawBytes));
            
            var pemfile =  TestContext.CurrentContext.TestDirectory + 
                           "/TestData/test-ed25519-pk.pem";
            var pk4 = PublicKey.FromPem(pemfile);
            Assert.AreEqual(KeyAlgo.ED25519, pk4.KeyAlgorithm);
            Assert.AreEqual(ED25519publicKey, pk4.ToAccountHex());
            Assert.IsTrue(Hex.Decode(ED25519publicKey)[1..].SequenceEqual(pk4.RawBytes));
            Assert.IsTrue(Hex.Decode(ED25519publicKey).SequenceEqual(pk4.GetBytes()));
            
            var hash3 = pk4.GetAccountHash();
            Assert.AreEqual(ED25519hash, hash3.Substring("account-hash-".Length), "Unexpected SECP256K1 hash value");
        }

        [Test]
        public void TestValidBlakeSecp256k1()
        {
            var publicKey = PublicKey.FromHexString(SECP256K1publicKey);
            Assert.AreEqual(KeyAlgo.SECP256K1, publicKey.KeyAlgorithm);
            
            var hash = publicKey.GetAccountHash();
            Assert.AreEqual(SECP256K1hash, hash.Substring("account-hash-".Length), "Unexpected SECP256K1hash value");
            
            var pk2 = PublicKey.FromBytes(Hex.Decode(SECP256K1publicKey));
            Assert.AreEqual(KeyAlgo.SECP256K1, pk2.KeyAlgorithm);
            Assert.AreEqual(SECP256K1publicKey, pk2.ToAccountHex());
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey)[1..].SequenceEqual(pk2.RawBytes));
            
            var pk3 = PublicKey.FromRawBytes(Hex.Decode(SECP256K1publicKey)[1..], KeyAlgo.SECP256K1);
            Assert.AreEqual(KeyAlgo.SECP256K1, pk3.KeyAlgorithm);
            Assert.AreEqual(SECP256K1publicKey, pk3.ToAccountHex());
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey)[1..].SequenceEqual(pk3.RawBytes));

            var pemfile =  TestContext.CurrentContext.TestDirectory + 
                           "/TestData/test-secp256k1-pk.pem";
            var pk4 = PublicKey.FromPem(pemfile);
            Assert.AreEqual(KeyAlgo.SECP256K1, pk4.KeyAlgorithm);
            Assert.AreEqual(SECP256K1publicKey, pk4.ToAccountHex());
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey)[1..].SequenceEqual(pk4.RawBytes));
            Assert.IsTrue(Hex.Decode(SECP256K1publicKey).SequenceEqual(pk4.GetBytes()));
            
            var hash3 = pk4.GetAccountHash();
            Assert.AreEqual(SECP256K1hash, hash3.Substring("account-hash-".Length), "Unexpected SECP256K1hash value");
        }
        
        [Test]
        public void TestInvalidEd25519()
        {
            // test that a public key without the algorithm identifier is not valid
            //
            var ex1 = Assert.Catch<ArgumentException>(() => PublicKey.FromHexString(
                ED25519publicKey.Substring(2).ToLower()));
            Assert.IsTrue(ex1?.Message.Contains("Wrong public key algorithm identifier"));
            
            // test that a public key with invalid length fails to create the object
            //
            var ex2 = Assert.Catch<ArgumentException>(() => PublicKey.FromHexString(
                ED25519publicKey.Substring(0, 64).ToLower()));
            Assert.IsTrue(ex2?.Message.Contains("Wrong public key format. Expected length is"));
            
            // test that an empty string fails
            //
            var ex3 = Assert.Catch<Exception>(() => PublicKey.FromHexString(""));
            Assert.IsNotNull(ex3);
        }
        
        [Test]
        public void TestInvalidChecksumEd25519()
        {
            var hex = ED25519publicKey.Substring(0, 32).ToLower() +
                      ED25519publicKey.Substring(32);
            var ex1 = Assert.Catch<ArgumentException>(() => PublicKey.FromHexString(hex));
            Assert.IsNotNull(ex1);
            Assert.IsTrue(ex1.Message.Contains("Public key checksum mismatch"));
        }

        [Test]
        public void TestInvalidChecksumSECP256K1()
        {
            var hex = SECP256K1publicKey.Substring(0, 32).ToLower() +
                      SECP256K1publicKey.Substring(32);
            var ex1 = Assert.Catch<ArgumentException>(() => PublicKey.FromHexString(hex));
            Assert.IsNotNull(ex1);
            Assert.IsTrue(ex1.Message.Contains("Public key checksum mismatch"));
        }

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

        [Test]
        public void TestWriteToPemEd25519()
        {
            var publicKey = PublicKey.FromHexString(ED25519publicKey);
            var tmpfile = Path.GetTempFileName();
            publicKey.WriteToPem(tmpfile);

            var pk2 = PublicKey.FromPem(tmpfile);
            Assert.AreEqual(ED25519publicKey, pk2.ToAccountHex());
        }

        [Test]
        public void TestWriteToPemSECP256K1()
        {
            var publicKey = PublicKey.FromHexString(SECP256K1publicKey);
            var tmpfile = Path.GetTempFileName();
            publicKey.WriteToPem(tmpfile);

            var pk2 = PublicKey.FromPem(tmpfile);
            Assert.AreEqual(SECP256K1publicKey, pk2.ToAccountHex());
        }

        [Test]
        public void TestPublicKeyEquality()
        {
            var pk1 = PublicKey.FromHexString(ED25519publicKey);
            
            Assert.IsTrue(pk1.Equals(PublicKey.FromBytes(pk1.GetBytes())));
            Assert.IsFalse(pk1.Equals(PublicKey.FromHexString("01b7c7c545dfa3fb853a97fb3581ce10eb4f67a5861abed6e70e5e3312fdde402c")));
        }
    }
}
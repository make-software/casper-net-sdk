using System;
using System.Linq;
using System.Net;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NuGet.Frameworks;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class GlobalStateKeyTests
    {
        private static  string ED25519publicKey = "019e7B8BDec03bA83Be4F5443d9f7f9111C77fec984Ce9Bb5bB7Eb3dA1e689c02D";
        private static  string ED25519hash = "6922db800A8772A734EcCeF19CE6C445EE3eBC984205053EAdAFa6C71191b0d1";
        private static  string SECP256K1publicKey = "020286F410e0c587e88E3b3297Fc860aa236D2de252675a6b1CdF3fC0FDc430e4183";
        private static  string SECP256K1hash = "584Aa5c753397A42A8aA2c7e4936D476042Afc445ff718eA8C20dA23434bDC80";

        [Test]
        public void CEP57PublicKeyTest()
        {
            int result; 
            
            // public key checksums are calculated without the algo identifier (ie. wo first byte)
            //
            var bytes = CEP57Checksum.Decode(ED25519publicKey.Substring(2), out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);

            var pk = PublicKey.FromRawBytes(bytes, KeyAlgo.ED25519);
            var accountHash = pk.GetAccountHash();
            Assert.AreEqual(ED25519hash, accountHash.Substring("account-hash-".Length));
            
            bytes = CEP57Checksum.Decode(SECP256K1publicKey.Substring(2), out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);

            pk = PublicKey.FromRawBytes(bytes, KeyAlgo.SECP256K1);
            accountHash = pk.GetAccountHash();
            Assert.AreEqual(SECP256K1hash, accountHash.Substring("account-hash-".Length));
        }
        
        [Test]
        public void TestAccountHash()
        {
            var sPublicKey = "01a35887f3962a6a232e8e11fa7d4567b6866d68850974aad7289ef287676825f6";
            var sAccountHash = "account-hash-De959C60bDC834acBD35244B6293c761f39a42C25a0DD09590a32274Dd8582Cc";
            
            var publicKey = PublicKey.FromHexString(sPublicKey);
            var accountHash = new AccountHashKey(publicKey).ToString();
            Assert.AreEqual(sAccountHash, accountHash);
        }

        [Test]
        public void CEP57LongByteArray()
        {
            var longHex = "0300000001381B36CD07aD85348607FFe0fa3A2d033Ea941d14763358EbeACe9c8ad3CB771" +
                          "01381B36CD07aD85348607FFe0fa3A2d033Ea941d14763358EbeACe9c8ad3CB771" +
                          "01381B36cd07ad85348607ffe0fa3A2d033Ea941d14763358EbeACe9c8ad3CB771";
            
            var bytes = CEP57Checksum.Decode(longHex, out var result);
            Assert.AreEqual(CEP57Checksum.NoChecksum, result);

            var encodedHex = CEP57Checksum.Encode(bytes);
            Assert.AreEqual(longHex.ToLower(), encodedHex);
        }
        
        [Test]
        public void CEP57DecodeEncode()
        {
            int result;

            var hash = "B8f1c7e68eCe8CdC01e8147A77BDEab4Ed92F04e6933EC35751Ad42D97B7f972";
            var bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            
            GlobalStateKey key = new HashKey(bytes);
            Assert.AreEqual($"hash-{hash}", key.ToString());

            hash = "D13D584363CA165E49bB24BAFf18a565fd66c740D4499a543946055743Ee11cE";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new TransferKey(bytes);
            Assert.AreEqual($"transfer-{hash}", key.ToString());

            hash = "87900329a66230B9f911B9dCd1323e90AF83FdE9Ba6AeB92C37adf22D865420c";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new DeployInfoKey(bytes);
            Assert.AreEqual($"deploy-{hash}", key.ToString());

            hash = "9fEd96c8F4adC27450A3E63916265EF00aaD7AC03ffE67Ff53015Adb15590894";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new BalanceKey(bytes);
            Assert.AreEqual($"balance-{hash}", key.ToString());

            hash = "7B9291078Bb22C387f452eE0B3ff1650005F666Fa108734d91e27D0cbEd0FD22";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new BidKey(bytes);
            Assert.AreEqual($"bid-{hash}", key.ToString());

            hash = "D13D584363CA165E49bB24BAFf18a565fd66c740D4499a543946055743Ee11cE";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new WithdrawKey(bytes);
            Assert.AreEqual($"withdraw-{hash}", key.ToString());
        }
    }
}
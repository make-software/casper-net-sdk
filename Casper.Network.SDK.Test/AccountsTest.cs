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
        private static  string ED25519publicKey = "019E7B8bDEc03bA83BE4f5443D9F7F9111C77fEC984cE9bb5BB7eB3Da1e689C02D";
        private static  string ED25519hash = "6922Db800a8772a734eCCef19cE6C445eE3ebC984205053eadafa6c71191b0d1";
        private static  string SECP256K1publicKey = "020286F410e0c587E88e3b3297Fc860aa236D2De252675A6b1CdF3Fc0fdc430e4183";
        private static  string SECP256K1hash = "584aA5C753397A42a8AA2c7e4936d476042AFc445FF718Ea8C20dA23434BDc80";

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
            var sPublicKey = "01a35887f3962a6A232e8E11fA7D4567B6866D68850974AaD7289Ef287676825F6";
            var sAccountHash = "account-hash-dE959c60bDc834AcbD35244B6293c761f39a42c25A0dd09590a32274DD8582cC";
            
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

            var hash = "66B754c5E2981B41D41af39B88C8583bD08EF176eB1a81F56b8F395685805968";
            var bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            
            GlobalStateKey key = new HashKey(bytes);
            Assert.AreEqual($"hash-{hash}", key.ToString());

            hash = "eA1D6C19ccAeb35Ae717065c250E0F7F6Dc64AC3c6494a797E0b33A23CA1f1b9";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new TransferKey(bytes);
            Assert.AreEqual($"transfer-{hash}", key.ToString());

            hash = "98d945f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new DeployInfoKey(bytes);
            Assert.AreEqual($"deploy-{hash}", key.ToString());

            hash = "8cf5E4aCF51f54Eb59291599187838Dc3BC234089c46fc6cA8AD17e762aE4401";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new BalanceKey(bytes);
            Assert.AreEqual($"balance-{hash}", key.ToString());

            hash = "010c3Fe81B7b862E50C77EF9A958a05BfA98444F26f96f23d37A13c96244cFB7";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new BidKey(bytes);
            Assert.AreEqual($"bid-{hash}", key.ToString());

            hash = "98d945f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6";
            bytes = CEP57Checksum.Decode(hash, out result);
            Assert.AreEqual(CEP57Checksum.ValidChecksum, result);
            Assert.AreEqual(hash, CEP57Checksum.Encode(bytes));

            key = new WithdrawKey(bytes);
            Assert.AreEqual($"withdraw-{hash}", key.ToString());
        }
    }
}
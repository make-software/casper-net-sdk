using System;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;

namespace NetCasperTest
{
    public class GlobalStateKeyTests
    {
        [Test]
        public void CEP57LongByteArray()
        {
            // checksum is not computed nor validated for hex strings longer
            // than 75 bytes (150 hex chars).
            
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

        [Test]
        public void CEP57HasChecksum()
        {
            const string hash = "010c3Fe81B7b862E50C77EF9A958a05BfA98444F26f96f23d37A13c96244cFB7";
            
            Assert.IsTrue(CEP57Checksum.HasChecksum(hash));
            Assert.IsFalse(CEP57Checksum.HasChecksum(hash.ToUpper()));
            Assert.IsFalse(CEP57Checksum.HasChecksum(hash.ToLower()));
            
            const string wrongHash = "nothexastring";

            var ex = Assert.Catch<Exception>(() => CEP57Checksum.HasChecksum(wrongHash));
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("Input is not an hexadecimal string"));
        }
    }
}
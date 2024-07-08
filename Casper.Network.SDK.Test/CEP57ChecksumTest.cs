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
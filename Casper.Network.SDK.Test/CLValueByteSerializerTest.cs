using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class CLValueByteSerializerTest
    {
        private CLValueByteSerializer serializer;
        [SetUp]
        public void SetUp()
        {
            serializer = new CLValueByteSerializer();
        }
        
        [Test]
        public void UnitByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.Unit());
            Assert.AreEqual("0000000009", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Unit()));
            Assert.AreEqual("01000000010d09", Hex.ToHexString(bytes));
        }

        [Test]
        public void OptionByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.Option(CLValue.Bool(false)));
            Assert.AreEqual("0200000001000d00", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Bool(true)));
            Assert.AreEqual("0200000001010d00", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.String("Hello, World!")));
            Assert.AreEqual("12000000010d00000048656c6c6f2c20576f726c64210d0a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.String(string.Empty)));
            Assert.AreEqual("0500000001000000000d0a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.I32(-10)));
            Assert.AreEqual("0500000001f6ffffff0d01", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.I64(-16)));
            Assert.AreEqual("0900000001f0ffffffffffffff0d02", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U64(10)));
            Assert.AreEqual("09000000010a000000000000000d05", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U512(10)));
            Assert.AreEqual("0300000001010a0d08", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(CLValue.U64(10)));
            Assert.AreEqual("09000000010a000000000000000d05", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.String));
            Assert.AreEqual("01000000000d0a", Hex.ToHexString(bytes));
        }

        [Test]
        public void URefByteSerializerTest()
        {
            var uref = "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007";
            var clValue = CLValue.URef(uref);
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("21000000000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f070c", Hex.ToHexString(bytes));

            clValue = CLValue.Option(CLValue.URef(uref));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("2200000001000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f070d0c", Hex.ToHexString(bytes));
        }

        [Test]
        public void Tuple1ByteSerializerTest()
        {
            var clValue = CLValue.Tuple1(CLValue.U32(17));
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("04000000110000001204", Hex.ToHexString(bytes));
            
            clValue = CLValue.Tuple1(CLValue.String("ABCDE"));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("09000000050000004142434445120a", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void Tuple2ByteSerializerTest()
        {
            var clValue = CLValue.Tuple2(CLValue.U32(17), CLValue.U32(127));
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("08000000110000007f000000130404", Hex.ToHexString(bytes));
            
            clValue = CLValue.Tuple2(CLValue.U32(127), CLValue.String("ABCDE"));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("0d0000007f00000005000000414243444513040a", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void Tuple3ByteSerializerTest()
        {
            var clValue = CLValue.Tuple3(CLValue.U32(17), CLValue.U32(127), CLValue.U32(17));
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("0c000000110000007f0000001100000014040404", Hex.ToHexString(bytes));
            
            clValue = CLValue.Tuple3(CLValue.U32(127), CLValue.String("ABCDE"), CLValue.U32(127));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("110000007f0000000500000041424344457f00000014040a04", Hex.ToHexString(bytes));
        }
    }
}
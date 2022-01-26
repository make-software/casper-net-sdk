using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
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
        public void BoolByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.Bool(false));
            Assert.AreEqual("010000000000", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Bool(true));
            Assert.AreEqual("010000000100", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Bool(false)));
            Assert.AreEqual("0200000001000d00", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Bool(true)));
            Assert.AreEqual("0200000001010d00", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.Bool));
            Assert.AreEqual("01000000000d00", Hex.ToHexString(bytes));
        }

        [Test]
        public void I32ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.I32(-10));
            Assert.AreEqual("04000000f6ffffff01", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.I32(-10)));
            Assert.AreEqual("0500000001f6ffffff0d01", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.I32));
            Assert.AreEqual("01000000000d01", Hex.ToHexString(bytes));
        }

        [Test]
        public void I64ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.I64(-16));
            Assert.AreEqual("08000000f0ffffffffffffff02", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.I64(-16)));
            Assert.AreEqual("0900000001f0ffffffffffffff0d02", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.I64));
            Assert.AreEqual("01000000000d02", Hex.ToHexString(bytes));
        }

        [Test]
        public void U8ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U8(0x00));
            Assert.AreEqual("010000000003", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.U8(0x7F));
            Assert.AreEqual("010000007f03", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U8(0xFF)));
            Assert.AreEqual("0200000001ff0d03", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U8));
            Assert.AreEqual("01000000000d03", Hex.ToHexString(bytes));
        }

        [Test]
        public void U32ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U32(uint.MaxValue));
            Assert.AreEqual("04000000ffffffff04", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U32(uint.MinValue)));
            Assert.AreEqual("0500000001000000000d04", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U32));
            Assert.AreEqual("01000000000d04", Hex.ToHexString(bytes));
        }

        [Test]
        public void U64ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U64(ulong.MaxValue));
            Assert.AreEqual("08000000ffffffffffffffff05", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U64(1)));
            Assert.AreEqual("090000000101000000000000000d05", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U64));
            Assert.AreEqual("01000000000d05", Hex.ToHexString(bytes));
        }

        [Test]
        public void U128ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U128(ulong.MaxValue));
            Assert.AreEqual("0900000008ffffffffffffffff06", Hex.ToHexString(bytes));

            var u128 = BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.HexNumber);
            u128 -= BigInteger.One;
            Assert.AreEqual(128, u128.GetBitLength());
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U128(u128)));
            Assert.AreEqual("120000000110feffffffffffffffffffffffffffffff0d06", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U128));
            Assert.AreEqual("01000000000d06", Hex.ToHexString(bytes));
        }

        [Test]
        public void U256ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U256(ulong.MaxValue));
            Assert.AreEqual("0900000008ffffffffffffffff07", Hex.ToHexString(bytes));

            var u256 = BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.HexNumber);
            u256 -= new BigInteger(0x80);
            Assert.AreEqual(256, u256.GetBitLength());
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U256(u256)));
            Assert.AreEqual("2200000001207fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff0d07", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U256));
            Assert.AreEqual("01000000000d07", Hex.ToHexString(bytes));
        }

        [Test]
        public void U512ByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.U512(ulong.MaxValue));
            Assert.AreEqual("0900000008ffffffffffffffff08", Hex.ToHexString(bytes));

            var u512 = BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" + "" +
                                        "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.HexNumber);
            u512 -= new BigInteger(0x80);
            Assert.AreEqual(512, u512.GetBitLength());
            bytes = serializer.ToBytes(CLValue.Option(CLValue.U512(u512)));
            Assert.AreEqual("4200000001407fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff" +
                            "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff0d08", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.U512));
            Assert.AreEqual("01000000000d08", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void UnitByteSerializerTest()
        {
            byte[] bytes = serializer.ToBytes(CLValue.Unit());
            Assert.AreEqual("0000000009", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Unit()));
            Assert.AreEqual("01000000010d09", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.Unit));
            Assert.AreEqual("01000000000d09", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void StringByteSerializerTest()
        {
            var bytes = serializer.ToBytes(CLValue.String("Hello, Casper!"));
            Assert.AreEqual("120000000e00000048656c6c6f2c20436173706572210a", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(CLValue.String("Hello, Casper!")));
            Assert.AreEqual("13000000010e00000048656c6c6f2c20436173706572210d0a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.String(string.Empty)));
            Assert.AreEqual("0500000001000000000d0a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.String));
            Assert.AreEqual("01000000000d0a", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void KeyByteSerializerTest()
        {
            var key = GlobalStateKey.FromString("account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7");
            var bytes = serializer.ToBytes(CLValue.Key(key));
            Assert.AreEqual("2100000000989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b70b", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(CLValue.Key(key)));
            Assert.AreEqual("220000000100989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b70d0b", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLKeyTypeInfo(KeyIdentifier.Account)));
            Assert.AreEqual("01000000000d0b", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void URefByteSerializerTest()
        {
            var uref = "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007";
            var bytes = serializer.ToBytes(CLValue.URef(uref));
            Assert.AreEqual("21000000000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f070c", 
                Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(CLValue.URef(uref)));
            Assert.AreEqual("2200000001000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f070d0c", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.URef));
            Assert.AreEqual("01000000000d0c", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void PublicKeyByteSerializerTest()
        {
            var publicKey =
                PublicKey.FromHexString("01381b36cd07Ad85348607ffE0fA3A2d033eA941D14763358eBEacE9C8aD3cB771");
            var bytes = serializer.ToBytes(CLValue.PublicKey(publicKey));
            Assert.AreEqual("2100000001381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb77116", 
                Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(CLValue.PublicKey(publicKey)));
            Assert.AreEqual("220000000101381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb7710d16", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(CLType.PublicKey));
            Assert.AreEqual("01000000000d16", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void ListByteSerializerTest()
        {
            var list = CLValue.List(new[]
            {
                CLValue.U32(1), CLValue.U32(2),
                CLValue.U32(3), CLValue.U32(4)
            });
            var bytes = serializer.ToBytes(list);
            Assert.AreEqual("1400000004000000010000000200000003000000040000000e04", 
                Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(list));
            Assert.AreEqual("150000000104000000010000000200000003000000040000000d0e04", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLListTypeInfo(CLType.U32)));
            Assert.AreEqual("01000000000d0e04", Hex.ToHexString(bytes));
        }

        [Test]
        public void EmptyListByteSerializerTest()
        {
            var list = CLValue.EmptyList(new CLKeyTypeInfo(KeyIdentifier.Hash));
            var bytes = serializer.ToBytes(list);
            Assert.AreEqual("04000000000000000e0b", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void ByteArrayKeyByteSerializerTest()
        {
            var value = Hex.Decode("0102030405060708");
            var bytes = serializer.ToBytes(CLValue.ByteArray(value));
            Assert.AreEqual("0800000001020304050607080f08000000", 
                Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(CLValue.ByteArray(value)));
            Assert.AreEqual("090000000101020304050607080d0f08000000", 
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLByteArrayTypeInfo(32)));
            Assert.AreEqual("01000000000d0f20000000", Hex.ToHexString(bytes));
        }

        [Test]
        public void MapByteSerializerTest()
        {
            var clValue = CLValue.Map(new Dictionary<CLValue, CLValue>()
            {
                {CLValue.String("key1"), CLValue.U32(1)}, {CLValue.String("key2"), CLValue.U32(2)}
            });
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("1c00000002000000040000006b65793101000000040000006b65793202000000110a04",
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("1d0000000102000000040000006b65793101000000040000006b657932020000000d110a04",
                Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLMapTypeInfo(CLType.String, CLType.U32)));
            Assert.AreEqual("01000000000d110a04", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void EmptyMapByteSerializerTest()
        {
            var list = CLValue.EmptyMap(CLType.String, new CLKeyTypeInfo(KeyIdentifier.Hash));
            var bytes = serializer.ToBytes(list);
            Assert.AreEqual("0400000000000000110a0b", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void ResultByteSerializerTest()
        {
            var clValue = CLValue.Ok(CLValue.U8(0xFF), new CLTypeInfo(CLType.String));
            var bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("0200000001ff10030a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("030000000101ff0d10030a", Hex.ToHexString(bytes));
            
            clValue = CLValue.Ok(CLValue.Unit(), new CLTypeInfo(CLType.String));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("010000000110090a", Hex.ToHexString(bytes));

            clValue = CLValue.Err(CLValue.String("Error!"), new CLTypeInfo(CLType.Unit));
            bytes = serializer.ToBytes(clValue);
            Assert.AreEqual("0b00000000060000004572726f722110090a", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("0c0000000100060000004572726f72210d10090a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(
                new CLResultTypeInfo( CLType.U8, CLType.String)));
            Assert.AreEqual("01000000000d10030a", Hex.ToHexString(bytes));
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
            
            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("0a000000010500000041424344450d120a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLTuple1TypeInfo(CLType.String)));
            Assert.AreEqual("01000000000d120a", Hex.ToHexString(bytes));
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

            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("0e000000017f0000000500000041424344450d13040a", Hex.ToHexString(bytes));
            
            bytes = serializer.ToBytes(CLValue.OptionNone(new CLTuple2TypeInfo(CLType.U32, CLType.String)));
            Assert.AreEqual("01000000000d13040a", Hex.ToHexString(bytes));
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

            bytes = serializer.ToBytes(CLValue.Option(clValue));
            Assert.AreEqual("12000000017f0000000500000041424344457f0000000d14040a04", Hex.ToHexString(bytes));

            bytes = serializer.ToBytes(CLValue.OptionNone(new CLTuple3TypeInfo(CLType.U32, CLType.String, CLType.U32)));
            Assert.AreEqual("01000000000d14040a04", Hex.ToHexString(bytes));
        }
    }
}
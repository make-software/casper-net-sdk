using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class BinaryReaderExtenstionsTest
    {
        [Test]
        public void ReadBoolTest()
        {
            var value = true;
            var bytes = new byte[] {0x01};
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<bool>(reader, CLType.Bool);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadInt32Test()
        {
            var value = int.MaxValue;
            var bytes = BitConverter.GetBytes(value);
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<int>(reader, CLType.I32);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadInt64Test()
        {
            var value = long.MinValue;
            var bytes = BitConverter.GetBytes(value);
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<long>(reader, CLType.I64);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadByteTest()
        {
            var bytes = new byte[] {0x00};
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<byte>(reader, CLType.U8);
            Assert.AreEqual(0x00, item);

            bytes = new byte[] {0x01};
            reader = new BinaryReader(new MemoryStream(bytes));

            item = BinaryReaderExtensions.ReadCLItem<byte>(reader, CLType.U8);
            Assert.AreEqual(0x01, item);
        }

        [Test]
        public void ReadUInt32Test()
        {
            var value = uint.MaxValue;
            var bytes = BitConverter.GetBytes(value);
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<uint>(reader, CLType.U32);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadUInt64Test()
        {
            var value = ulong.MinValue;
            var bytes = BitConverter.GetBytes(value);
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<ulong>(reader, CLType.U64);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadBigIntegerTest()
        {
            var value = new BigInteger(Int64.MaxValue - 1);
            var bytes = Hex.Decode("08FEFFFFFFFFFFFF7F");
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<BigInteger>(reader, CLType.U512);
            Assert.AreEqual(value, item);

            value = new BigInteger(UInt64.MaxValue - 1);
            bytes = Hex.Decode("08FEFFFFFFFFFFFFFF");
            reader = new BinaryReader(new MemoryStream(bytes));

            item = BinaryReaderExtensions.ReadCLItem<BigInteger>(reader, CLType.U512);
            Assert.AreEqual(value, item);

            value = new BigInteger(UInt64.MaxValue - 1);
            bytes = Hex.Decode("09FEFFFFFFFFFFFFFF00");
            reader = new BinaryReader(new MemoryStream(bytes));

            item = BinaryReaderExtensions.ReadCLItem<BigInteger>(reader, CLType.U512);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadStringTest()
        {
            var value = "Hello world.";
            var bytes = new byte[4 + value.Length];
            bytes[0] = 0x0c;
            Array.Copy(System.Text.Encoding.UTF8.GetBytes(value), 0, bytes, 4, value.Length);

            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<string>(reader, CLType.String);
            Assert.AreEqual(value, item);
        }

        [Test]
        public void ReadURefTest()
        {
            var value = new URef(
                "uref-0001020304050607000102030405060700010203040506070001020304050607-007");
            var bytes = Hex.Decode("000102030405060700010203040506070001020304050607000102030405060707");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<URef>(reader, CLType.URef);
            Assert.AreEqual(value.ToHexString(), item.ToHexString());
        }

        [Test]
        public void ReadPublicKeyTest()
        {
            var key = KeyPair.CreateNew(KeyAlgo.ED25519);
            var bytes = key.PublicKey.GetBytes();

            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<PublicKey>(reader, CLType.PublicKey);
            Assert.AreEqual(key.PublicKey.ToAccountHex(), item.ToAccountHex());
        }

        [Test]
        public void ReadGlobalStateKeyTest()
        {
            // test with an AccountHash key
            //
            var key = KeyPair.CreateNew(KeyAlgo.ED25519);
            var accHash = key.PublicKey.GetAccountHash();
            var gsKey = GlobalStateKey.FromString(accHash);

            var reader = new BinaryReader(new MemoryStream(gsKey.GetBytes()));

            var item = BinaryReaderExtensions.ReadCLItem<GlobalStateKey>(reader,
                new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual(gsKey.ToString(), item.ToString());

            // test with an EraInfo key
            //
            gsKey = GlobalStateKey.FromString("era-2034");
            reader = new BinaryReader(new MemoryStream(gsKey.GetBytes()));

            item = BinaryReaderExtensions.ReadCLItem<GlobalStateKey>(reader, new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual(gsKey.ToString(), item.ToString());

            // test with an URef key
            //
            gsKey = GlobalStateKey.FromString(
                "uref-0001020304050607000102030405060700010203040506070001020304050607-007");
            reader = new BinaryReader(new MemoryStream(gsKey.GetBytes()));

            item = BinaryReaderExtensions.ReadCLItem<GlobalStateKey>(reader, new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual(gsKey.ToString(), item.ToString());
        }

        [Test]
        public void ReadOptionNoneTest()
        {
            // Option(String) = None
            //
            var bytes = new byte[] {0x00};
            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<string>(reader,
                new CLOptionTypeInfo(CLType.String));
            Assert.IsNull(item);

            reader = new BinaryReader(new MemoryStream(bytes));

            // Option(I32) = None
            //
            var item2 = BinaryReaderExtensions.ReadCLItem<int?>(reader,
                new CLOptionTypeInfo(CLType.I32));
            Assert.IsNull(item2);
        }

        [Test]
        public void ReadOptionSomeTest()
        {
            // Option(String) = "Hello world."
            //
            var value = "Hello world.";
            var bytes = new byte[5 + value.Length];
            bytes[0] = 0x01; // Some()
            bytes[1] = 0x0c; // string length
            Array.Copy(System.Text.Encoding.UTF8.GetBytes(value), 0, bytes, 5, value.Length);

            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<string>(reader,
                new CLOptionTypeInfo(CLType.String));

            Assert.IsNotNull(item);
            Assert.AreEqual(value, item);

            // Option(I32) = 10
            //
            bytes = Hex.Decode("010a000000");

            reader = new BinaryReader(new MemoryStream(bytes));

            var item2 = BinaryReaderExtensions.ReadCLItem<int?>(reader,
                new CLOptionTypeInfo(CLType.I32));

            Assert.IsNotNull(item2);
            Assert.AreEqual(10, item2);
        }

        [Test]
        public void ReadByteArrayTest()
        {
            var bytes = Hex.Decode("0102030405060708");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var item = BinaryReaderExtensions.ReadCLItem<byte[]>(reader,
                new CLByteArrayTypeInfo(8));

            Assert.IsNotNull(item);
            Assert.IsTrue(item.SequenceEqual(bytes));
        }

        [Test]
        public void ReadListOfByteArrayTest()
        {
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090A0B0C0D0E0F00");
            var bytes = Hex.Decode("020000000102030405060708090A0B0C0D0E0F00");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLListTypeInfo(new CLByteArrayTypeInfo(8));

            var item = BinaryReaderExtensions.ReadCLItem<List<byte[]>>(reader,
                clType);

            Assert.IsNotNull(item);
            Assert.IsTrue(item[0].SequenceEqual(p1));
            Assert.IsTrue(item[1].SequenceEqual(p2));

            reader = new BinaryReader(new MemoryStream(bytes));

            var item2 = BinaryReaderExtensions.ReadCLItem(reader, clType, null);
            var list = item2 as IList;
            Assert.IsNotNull(list);
            var b1 = list[0] as byte[];
            Assert.IsNotNull(b1);
            Assert.IsTrue(b1.SequenceEqual(p1));
            var b2 = list[1] as byte[];
            Assert.IsNotNull(b2);
            Assert.IsTrue(b2.SequenceEqual(p2));
        }

        [Test]
        public void ReadDictionaryGenericTest()
        {
            var k1 = "map1key1";
            var k2 = "map1key2";
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090A0B0C0D0E0F00");
            var bytes = Hex.Decode(
                "02000000080000006d6170316b6579310102030405060708080000006d6170316b657932090A0B0C0D0E0F00");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLMapTypeInfo(CLType.String,
                new CLByteArrayTypeInfo(8));

            var item = BinaryReaderExtensions.ReadCLItem<Dictionary<string, byte[]>>(reader,
                clType);
            Assert.IsNotNull(item);
            Assert.IsTrue(item.ContainsKey(k1));
            Assert.IsTrue(item.ContainsKey(k2));
            Assert.IsTrue(item[k1].SequenceEqual(p1));
            Assert.IsTrue(item[k2].SequenceEqual(p2));

            reader = new BinaryReader(new MemoryStream(bytes));

            var item2 = BinaryReaderExtensions.ReadCLItem(reader, clType, null);
            var dict = item2 as IDictionary;
            Assert.IsNotNull(dict);
            var b1 = dict[k1] as byte[];
            Assert.IsNotNull(b1);
            Assert.IsTrue(b1.SequenceEqual(p1));
            var b2 = dict[k2] as byte[];
            Assert.IsNotNull(b2);
            Assert.IsTrue(b2.SequenceEqual(p2));
        }

        [Test]
        public void ReadResultInt32Test()
        {
            var bytes = Hex.Decode("01FEFFFF7F");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLResultTypeInfo(CLType.I32, CLType.String);

            var item = BinaryReaderExtensions.ReadCLItem<Result<int, string>>(reader, clType);

            Assert.IsNotNull(item);
            Assert.IsTrue(item.Success);
            Assert.IsNotNull(item.Value);
            Assert.AreEqual(int.MaxValue - 1, item.Value);
            Assert.IsNull(item.Error);

            bytes = Hex.Decode("00070000004661696c757265");

            reader = new BinaryReader(new MemoryStream(bytes));

            item = BinaryReaderExtensions.ReadCLItem<Result<int, string>>(reader, clType);

            Assert.IsNotNull(item);
            Assert.IsFalse(item.Success);
            Assert.AreEqual(default(int), item.Value);
            Assert.IsNotNull(item.Error);
            Assert.AreEqual("Failure", item.Error);
        }

        [Test]
        public void ReadResultUnitTest()
        {
            var bytes = new byte[] {0x01};
            
            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType =new CLResultTypeInfo(CLType.Unit, CLType.String);
            
            var result = BinaryReaderExtensions.ReadCLItem<Result<Unit, string>>(reader, clType);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.IsFailure);
            Assert.AreEqual(Unit.Default, result.Value);
        }
        
        [Test]
        public void ReadListResultTest()
        {
            var bytes = Hex.Decode("0200000001FEFFFF7F00070000004661696c757265");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLListTypeInfo(
                new CLResultTypeInfo(CLType.I32, CLType.String));

            var list = BinaryReaderExtensions.ReadCLItem<List<Result<int, string>>>(reader, clType);

            var item1 = list[0];
            Assert.IsNotNull(item1);
            Assert.IsTrue(item1.Success);
            Assert.IsNotNull(item1.Value);
            Assert.AreEqual(int.MaxValue - 1, item1.Value);
            Assert.IsNull(item1.Error);

            var item2 = list[1];
            Assert.IsNotNull(item2);
            Assert.IsFalse(item2.Success);
            Assert.AreEqual(default(int), item2.Value);
            Assert.IsNotNull(item2.Error);
            Assert.AreEqual("Failure", item2.Error);
        }

        [Test]
        public void ReadMapListTest()
        {
            var k1 = "map1key1";
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090A0B0C0D0E0F00");
            
            // Ok(Map(String,List(ByteArray)))
            //
            var bytes = Hex.Decode("0101000000080000006d6170316b657931020000000102030405060708090A0B0C0D0E0F00");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLResultTypeInfo(
                new CLMapTypeInfo(CLType.String,
                    new CLListTypeInfo(new CLByteArrayTypeInfo(8))), CLType.String);

            var result = BinaryReaderExtensions.ReadCLItem <Result<Dictionary<string, List<byte[]>>, string>>(reader, clType);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(default(string),result.Error);
            
            var dict = result.Value;
            Assert.IsNotNull(dict);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey(k1));

            var list = dict[k1];
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list[0].SequenceEqual(p1));
            Assert.IsTrue(list[1].SequenceEqual(p2));
        }

        [Test]
        public void ReadOptionListTest()
        {
            var k1 = "map1key1";
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090A0B0C0D0E0F00");
            
            // Some(Map(String,List(ByteArray)))
            //
            var bytes = Hex.Decode("0101000000080000006d6170316b657931020000000102030405060708090A0B0C0D0E0F00");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLOptionTypeInfo(
                new CLMapTypeInfo(CLType.String,
                    new CLListTypeInfo(new CLByteArrayTypeInfo(8))));

            var dict = BinaryReaderExtensions.ReadCLItem <Dictionary<string, List<byte[]>>>(reader, clType);
            
            Assert.IsNotNull(dict);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey(k1));
            
            var list = dict[k1];
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list[0].SequenceEqual(p1));
            Assert.IsTrue(list[1].SequenceEqual(p2));
        }

        [Test]
        public void ReadTuple1Test()
        {
            var bytes = Hex.Decode("02000000080000006d6170316b6579317f080000006d6170316b65793280");
            
            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLTuple1TypeInfo(new CLMapTypeInfo(CLType.String,CLType.U8));
            
            var tuple1 = BinaryReaderExtensions.ReadCLItem<Tuple<Dictionary<string,byte>>>(reader, clType);

            Assert.IsNotNull(tuple1);

            var dict = tuple1.Item1;
            Assert.IsNotNull(dict);
            Assert.IsTrue(dict.ContainsKey("map1key1"));
            Assert.IsTrue(dict.ContainsKey("map1key2"));
            Assert.AreEqual(127, dict["map1key1"]);
            Assert.AreEqual(128, dict["map1key2"]);
        }

        [Test]
        public void ReadTuple2Test()
        {
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090A0B0C0D0E0F00");

            var bytes = Hex.Decode("7b000000020000000102030405060708090A0B0C0D0E0F00");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLTuple2TypeInfo(CLType.I32, new CLListTypeInfo(new CLByteArrayTypeInfo(8)));
            
            var tuple2 = BinaryReaderExtensions.ReadCLItem<Tuple<int, List<byte[]>>>(reader, clType);

            Assert.IsNotNull(tuple2);
            Assert.AreEqual(123, tuple2.Item1);

            var list = tuple2.Item2;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list[0].SequenceEqual(p1));
            Assert.IsTrue(list[1].SequenceEqual(p2));
        }
        
        [Test]
        public void ReadTuple3Test()
        {
            var bytes = Hex.Decode("7b00000001cdd5422295f6a61e86a4d3229b28dac2e67523c41e2aafed3a041362df7a843207");

            var reader = new BinaryReader(new MemoryStream(bytes));

            var clType = new CLTuple3TypeInfo(CLType.I32, CLType.Bool, CLType.URef);
            
            var tuple3 = BinaryReaderExtensions.ReadCLItem <Tuple<int, bool, URef>>(reader, clType);

            Assert.IsNotNull(tuple3);
            Assert.AreEqual(123, tuple3.Item1);
            Assert.AreEqual(true, tuple3.Item2);

            var uref = new URef("uref-cdd5422295f6a61e86a4d3229b28dac2e67523c41e2aafed3a041362df7a8432-007");
            Assert.AreEqual(uref.ToHexString(), tuple3.Item3.ToHexString());
        }
    }
}
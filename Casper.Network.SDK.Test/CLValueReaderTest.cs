using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class CLValueReaderTest
    {
        // ─── helpers ───────────────────────────────────────────────────────────────

        private static CLValueReader ReaderFor(byte[] bytes) =>
            new CLValueReader(new BinaryReader(new MemoryStream(bytes)));

        // ─── constructor guard ─────────────────────────────────────────────────────

        [Test]
        public void NullReaderThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CLValueReader(null));
        }

        [Test]
        public void NullTypeInfoThrowsArgumentNullException()
        {
            var reader = ReaderFor(Array.Empty<byte>());
            Assert.Throws<ArgumentNullException>(() => reader.Read(null));
        }

        // ─── primitives ────────────────────────────────────────────────────────────

        [Test]
        public void ReadBoolTest()
        {
            foreach (var value in new[] { false, true })
            {
                var original = CLValue.Bool(value);
                var result = ReaderFor(original.Bytes).Read(CLType.Bool);

                Assert.AreEqual(CLType.Bool, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToBoolean());
            }
        }

        [Test]
        public void ReadI32Test()
        {
            foreach (var value in new[] { int.MinValue, -10, 0, 42, int.MaxValue })
            {
                var original = CLValue.I32(value);
                var result = ReaderFor(original.Bytes).Read(CLType.I32);

                Assert.AreEqual(CLType.I32, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToInt32());
            }
        }

        [Test]
        public void ReadI64Test()
        {
            foreach (var value in new[] { long.MinValue, -16L, 0L, 1L, long.MaxValue })
            {
                var original = CLValue.I64(value);
                var result = ReaderFor(original.Bytes).Read(CLType.I64);

                Assert.AreEqual(CLType.I64, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToInt64());
            }
        }

        [Test]
        public void ReadU8Test()
        {
            foreach (var value in new byte[] { 0x00, 0x7F, 0xFF })
            {
                var original = CLValue.U8(value);
                var result = ReaderFor(original.Bytes).Read(CLType.U8);

                Assert.AreEqual(CLType.U8, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToByte());
            }
        }

        [Test]
        public void ReadU32Test()
        {
            foreach (var value in new uint[] { 0, 1, uint.MaxValue })
            {
                var original = CLValue.U32(value);
                var result = ReaderFor(original.Bytes).Read(CLType.U32);

                Assert.AreEqual(CLType.U32, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToUInt32());
            }
        }

        [Test]
        public void ReadU64Test()
        {
            foreach (var value in new ulong[] { 0, 1, ulong.MaxValue })
            {
                var original = CLValue.U64(value);
                var result = ReaderFor(original.Bytes).Read(CLType.U64);

                Assert.AreEqual(CLType.U64, result.TypeInfo.Type);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
                Assert.AreEqual(value, result.ToUInt64());
            }
        }

        [Test]
        public void ReadU128Test()
        {
            // single-byte value
            var small = new BigInteger(255);
            var original = CLValue.U128(small);
            var result = ReaderFor(original.Bytes).Read(CLType.U128);
            Assert.AreEqual(CLType.U128, result.TypeInfo.Type);
            Assert.AreEqual(small, result.ToBigInteger());

            // ulong.MaxValue — multi-byte encoding
            var large = new BigInteger(ulong.MaxValue);
            original = CLValue.U128(large);
            result = ReaderFor(original.Bytes).Read(CLType.U128);
            Assert.AreEqual(CLType.U128, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(large, result.ToBigInteger());
        }

        [Test]
        public void ReadU256Test()
        {
            var value = new BigInteger(ulong.MaxValue);
            var original = CLValue.U256(value);
            var result = ReaderFor(original.Bytes).Read(CLType.U256);

            Assert.AreEqual(CLType.U256, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(value, result.ToBigInteger());
        }

        [Test]
        public void ReadU512Test()
        {
            var value = new BigInteger(ulong.MaxValue);
            var original = CLValue.U512(value);
            var result = ReaderFor(original.Bytes).Read(CLType.U512);

            Assert.AreEqual(CLType.U512, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(value, result.ToBigInteger());
        }

        [Test]
        public void ReadUnitTest()
        {
            var original = CLValue.Unit();
            var result = ReaderFor(original.Bytes).Read(CLType.Unit);

            Assert.AreEqual(CLType.Unit, result.TypeInfo.Type);
            Assert.AreEqual(0, result.Bytes.Length);
        }

        // ─── string ────────────────────────────────────────────────────────────────

        [Test]
        public void ReadStringTest()
        {
            var value = "Hello, Casper!";
            var original = CLValue.String(value);
            var result = ReaderFor(original.Bytes).Read(CLType.String);

            Assert.AreEqual(CLType.String, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(value, result.ToString());
        }

        [Test]
        public void ReadEmptyStringTest()
        {
            var original = CLValue.String(string.Empty);
            var result = ReaderFor(original.Bytes).Read(CLType.String);

            Assert.AreEqual(CLType.String, result.TypeInfo.Type);
            Assert.AreEqual(string.Empty, result.ToString());
        }

        // ─── URef ──────────────────────────────────────────────────────────────────

        [Test]
        public void ReadURefTest()
        {
            var urefStr = "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007";
            var original = CLValue.URef(urefStr);
            var result = ReaderFor(original.Bytes).Read(CLType.URef);

            Assert.AreEqual(CLType.URef, result.TypeInfo.Type);
            Assert.AreEqual(33, result.Bytes.Length);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(urefStr, result.ToURef().ToString());
        }

        // ─── PublicKey ─────────────────────────────────────────────────────────────

        [Test]
        public void ReadPublicKeyEd25519Test()
        {
            var publicKey = KeyPair.CreateNew(KeyAlgo.ED25519).PublicKey;
            var original = CLValue.PublicKey(publicKey);
            var result = ReaderFor(original.Bytes).Read(CLType.PublicKey);

            Assert.AreEqual(CLType.PublicKey, result.TypeInfo.Type);
            Assert.AreEqual(33, result.Bytes.Length);                      // 1 algo + 32 key bytes
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(publicKey.ToAccountHex(), result.ToPublicKey().ToAccountHex());
        }

        [Test]
        public void ReadPublicKeySecp256k1Test()
        {
            var publicKey = KeyPair.CreateNew(KeyAlgo.SECP256K1).PublicKey;
            var original = CLValue.PublicKey(publicKey);
            var result = ReaderFor(original.Bytes).Read(CLType.PublicKey);

            Assert.AreEqual(CLType.PublicKey, result.TypeInfo.Type);
            Assert.AreEqual(34, result.Bytes.Length);                      // 1 algo + 33 key bytes
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(publicKey.ToAccountHex(), result.ToPublicKey().ToAccountHex());
        }

        // ─── Key (GlobalStateKey) ──────────────────────────────────────────────────

        [Test]
        public void ReadKeyAccountHashTest()
        {
            var key = GlobalStateKey.FromString(
                "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7");
            var original = CLValue.Key(key);
            var typeInfo = new CLKeyTypeInfo(KeyIdentifier.Account);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Key, result.TypeInfo.Type);
            Assert.AreEqual(33, result.Bytes.Length);                      // 1 tag + 32 hash bytes
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(key.ToString(), result.ToGlobalStateKey().ToString());
        }

        [Test]
        public void ReadKeyEraInfoTest()
        {
            var key = GlobalStateKey.FromString("era-2034");
            var original = CLValue.Key(key);
            var typeInfo = new CLKeyTypeInfo(KeyIdentifier.EraInfo);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Key, result.TypeInfo.Type);
            Assert.AreEqual(9, result.Bytes.Length);                       // 1 tag + 8 u64 bytes
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(key.ToString(), result.ToGlobalStateKey().ToString());
        }

        [Test]
        public void ReadKeyURefTest()
        {
            var key = GlobalStateKey.FromString(
                "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007");
            var original = CLValue.Key(key);
            var typeInfo = new CLKeyTypeInfo(KeyIdentifier.URef);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Key, result.TypeInfo.Type);
            Assert.AreEqual(34, result.Bytes.Length);                      // 1 tag + 33 URef bytes
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(key.ToString(), result.ToGlobalStateKey().ToString());
        }

        // ─── Option ────────────────────────────────────────────────────────────────

        [Test]
        public void ReadOptionSomeTest()
        {
            var original = CLValue.Option(CLValue.U32(42u));
            var typeInfo = new CLOptionTypeInfo(CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Option, result.TypeInfo.Type);
            Assert.AreEqual(CLType.U32, ((CLOptionTypeInfo)result.TypeInfo).OptionType.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.IsTrue(result.IsSome());
            Assert.AreEqual(0x01, result.Bytes[0]);                        // Some tag
            // inner bytes after the tag should match U32(42).Bytes
            var innerBytes = result.Bytes.Skip(1).ToArray();
            Assert.AreEqual(Hex.ToHexString(CLValue.U32(42u).Bytes), Hex.ToHexString(innerBytes));
        }

        [Test]
        public void ReadOptionNoneTest()
        {
            var original = CLValue.OptionNone(CLType.U32);
            var typeInfo = new CLOptionTypeInfo(CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Option, result.TypeInfo.Type);
            Assert.AreEqual(CLType.U32, ((CLOptionTypeInfo)result.TypeInfo).OptionType.Type);
            Assert.AreEqual(1, result.Bytes.Length);
            Assert.AreEqual(0x00, result.Bytes[0]);                        // None tag
            Assert.IsTrue(result.IsNone());
        }

        // ─── List ──────────────────────────────────────────────────────────────────

        [Test]
        public void ReadListTest()
        {
            var original = CLValue.List(new[]
            {
                CLValue.U32(1), CLValue.U32(2), CLValue.U32(3), CLValue.U32(4)
            });
            var typeInfo = new CLListTypeInfo(CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.List, result.TypeInfo.Type);
            Assert.AreEqual(CLType.U32, ((CLListTypeInfo)result.TypeInfo).ListType.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));

            var items = result.ToList<uint>();
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(1u, items[0]);
            Assert.AreEqual(2u, items[1]);
            Assert.AreEqual(3u, items[2]);
            Assert.AreEqual(4u, items[3]);
        }

        [Test]
        public void ReadEmptyListTest()
        {
            var original = CLValue.EmptyList(new CLKeyTypeInfo(KeyIdentifier.Hash));
            var typeInfo = new CLListTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Hash));
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.List, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(0, result.ToList().Count);
        }

        // ─── ByteArray ─────────────────────────────────────────────────────────────

        [Test]
        public void ReadByteArrayTest()
        {
            var data = Hex.Decode("0102030405060708");
            var original = CLValue.ByteArray(data);
            var typeInfo = new CLByteArrayTypeInfo(8);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.ByteArray, result.TypeInfo.Type);
            Assert.AreEqual(8, result.Bytes.Length);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.IsTrue(data.SequenceEqual(result.ToByteArray()));
        }

        // ─── Result ────────────────────────────────────────────────────────────────

        [Test]
        public void ReadResultOkTest()
        {
            var original = CLValue.Ok(CLValue.U8(0xFF), new CLTypeInfo(CLType.String));
            var typeInfo = new CLResultTypeInfo(CLType.U8, CLType.String);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Result, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(0x01, result.Bytes[0]);                        // Ok tag
            var typed = result.ToResult<byte, string>();
            Assert.IsTrue(typed.Success);
            Assert.AreEqual(0xFF, typed.Value);
        }

        [Test]
        public void ReadResultErrTest()
        {
            var original = CLValue.Err(CLValue.String("Error!"), new CLTypeInfo(CLType.Unit));
            var typeInfo = new CLResultTypeInfo(CLType.Unit, CLType.String);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Result, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(0x00, result.Bytes[0]);                        // Err tag
            var typed = result.ToResult<Unit, string>();
            Assert.IsFalse(typed.Success);
            Assert.AreEqual("Error!", typed.Error);
        }

        // ─── Map ───────────────────────────────────────────────────────────────────

        [Test]
        public void ReadMapTest()
        {
            var original = CLValue.Map(new Dictionary<CLValue, CLValue>
            {
                { CLValue.String("key1"), CLValue.U32(1) },
                { CLValue.String("key2"), CLValue.U32(2) }
            });
            var typeInfo = new CLMapTypeInfo(CLType.String, CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Map, result.TypeInfo.Type);
            var mapType = (CLMapTypeInfo)result.TypeInfo;
            Assert.AreEqual(CLType.String, mapType.KeyType.Type);
            Assert.AreEqual(CLType.U32, mapType.ValueType.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));

            var dict = result.ToDictionary<string, uint>();
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(1u, dict["key1"]);
            Assert.AreEqual(2u, dict["key2"]);
        }

        [Test]
        public void ReadEmptyMapTest()
        {
            var original = CLValue.EmptyMap(CLType.String, new CLKeyTypeInfo(KeyIdentifier.Hash));
            var typeInfo = new CLMapTypeInfo(CLType.String, new CLKeyTypeInfo(KeyIdentifier.Hash));
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Map, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(0, result.ToDictionary().Count);
        }

        // ─── Tuples ────────────────────────────────────────────────────────────────

        [Test]
        public void ReadTuple1Test()
        {
            var original = CLValue.Tuple1(CLValue.U32(17));
            var typeInfo = new CLTuple1TypeInfo(CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Tuple1, result.TypeInfo.Type);
            Assert.AreEqual(CLType.U32, ((CLTuple1TypeInfo)result.TypeInfo).Type0.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.AreEqual(17u, result.ToTuple1<uint>().Item1);
        }

        [Test]
        public void ReadTuple2Test()
        {
            var original = CLValue.Tuple2(CLValue.U32(17), CLValue.U32(127));
            var typeInfo = new CLTuple2TypeInfo(CLType.U32, CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Tuple2, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            var t = result.ToTuple2<uint, uint>();
            Assert.AreEqual(17u, t.Item1);
            Assert.AreEqual(127u, t.Item2);
        }

        [Test]
        public void ReadTuple2MixedTypesTest()
        {
            var original = CLValue.Tuple2(CLValue.U32(127), CLValue.String("ABCDE"));
            var typeInfo = new CLTuple2TypeInfo(CLType.U32, CLType.String);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Tuple2, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            var t = result.ToTuple2<uint, string>();
            Assert.AreEqual(127u, t.Item1);
            Assert.AreEqual("ABCDE", t.Item2);
        }

        [Test]
        public void ReadTuple3Test()
        {
            var original = CLValue.Tuple3(CLValue.U32(17), CLValue.U32(127), CLValue.U32(255));
            var typeInfo = new CLTuple3TypeInfo(CLType.U32, CLType.U32, CLType.U32);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Tuple3, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            var t = result.ToTuple3<uint, uint, uint>();
            Assert.AreEqual(17u, t.Item1);
            Assert.AreEqual(127u, t.Item2);
            Assert.AreEqual(255u, t.Item3);
        }

        // ─── nested / compound types ───────────────────────────────────────────────

        [Test]
        public void ReadOptionSomeListTest()
        {
            // Option(Some(List(U32)))
            var inner = CLValue.List(new[] { CLValue.U32(10), CLValue.U32(20) });
            var original = CLValue.Option(inner);
            var typeInfo = new CLOptionTypeInfo(new CLListTypeInfo(CLType.U32));
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Option, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            Assert.IsTrue(result.IsSome());
        }

        [Test]
        public void ReadOptionNoneListTest()
        {
            // Option(None) with inner type List(U32)
            var original = CLValue.OptionNone(new CLListTypeInfo(CLType.U32));
            var typeInfo = new CLOptionTypeInfo(new CLListTypeInfo(CLType.U32));
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Option, result.TypeInfo.Type);
            Assert.IsTrue(result.IsNone());
        }

        [Test]
        public void ReadListOfByteArraysTest()
        {
            // List(ByteArray(8))
            var p1 = Hex.Decode("0102030405060708");
            var p2 = Hex.Decode("090a0b0c0d0e0f00");
            var original = CLValue.List(new[]
            {
                CLValue.ByteArray(p1),
                CLValue.ByteArray(p2)
            });
            var typeInfo = new CLListTypeInfo(new CLByteArrayTypeInfo(8));
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.List, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));

            var items = result.ToList<byte[]>();
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(p1.SequenceEqual(items[0]));
            Assert.IsTrue(p2.SequenceEqual(items[1]));
        }

        [Test]
        public void ReadTuple3MixedComplexTypesTest()
        {
            // Tuple3(I32, Bool, URef)
            var uref = "uref-cdd5422295f6a61e86a4d3229b28dac2e67523c41e2aafed3a041362df7a8432-007";
            var original = CLValue.Tuple3(
                CLValue.I32(123),
                CLValue.Bool(true),
                CLValue.URef(uref));
            var typeInfo = new CLTuple3TypeInfo(CLType.I32, CLType.Bool, CLType.URef);
            var result = ReaderFor(original.Bytes).Read(typeInfo);

            Assert.AreEqual(CLType.Tuple3, result.TypeInfo.Type);
            Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes));
            var t = result.ToTuple3<int, bool, URef>();
            Assert.AreEqual(123, t.Item1);
            Assert.AreEqual(true, t.Item2);
            Assert.AreEqual(uref, t.Item3.ToString());
        }

        // ─── stream position and multi-value reads ─────────────────────────────────

        [Test]
        public void ReadSequentialValuesAdvancesStreamTest()
        {
            // Pack three values back-to-back into a single MemoryStream and verify
            // that each Read() advances the position so the next call reads correctly.
            var u32Bytes   = CLValue.U32(42u).Bytes;
            var boolBytes  = CLValue.Bool(true).Bytes;
            var strBytes   = CLValue.String("Casper").Bytes;

            using var ms = new MemoryStream();
            ms.Write(u32Bytes,  0, u32Bytes.Length);
            ms.Write(boolBytes, 0, boolBytes.Length);
            ms.Write(strBytes,  0, strBytes.Length);
            ms.Position = 0;

            var clValueReader = new CLValueReader(new BinaryReader(ms));

            var v1 = clValueReader.Read(CLType.U32);
            var v2 = clValueReader.Read(CLType.Bool);
            var v3 = clValueReader.Read(CLType.String);

            // All bytes consumed — stream at end
            Assert.AreEqual(ms.Length, ms.Position);

            Assert.AreEqual(42u,      v1.ToUInt32());
            Assert.AreEqual(true,     v2.ToBoolean());
            Assert.AreEqual("Casper", v3.ToString());
        }

        // ─── bytes match CLValue factory output ────────────────────────────────────

        [Test]
        public void ReadBytesMatchCLValueFactoryOutputTest()
        {
            // CLValueReader.Read() must produce exactly the same Bytes as the
            // corresponding CLValue factory method — verified against known hex strings
            // from CLValueByteSerializerTest (the data-only portion, no length/type prefix).

            var serializer = new CLValueByteSerializer();

            void VerifyRoundTrip(CLValue original, CLTypeInfo typeInfo)
            {
                var result = ReaderFor(original.Bytes).Read(typeInfo);
                Assert.AreEqual(Hex.ToHexString(original.Bytes), Hex.ToHexString(result.Bytes),
                    $"Bytes mismatch for CLType {typeInfo.Type}");
                // Also verify that the result can be re-serialized identically
                var reEncoded = serializer.ToBytes(result);
                var originalEncoded = serializer.ToBytes(original);
                Assert.AreEqual(Hex.ToHexString(originalEncoded), Hex.ToHexString(reEncoded),
                    $"Re-serialized bytes mismatch for CLType {typeInfo.Type}");
            }

            VerifyRoundTrip(CLValue.Bool(false),                            CLType.Bool);
            VerifyRoundTrip(CLValue.I32(-10),                               CLType.I32);
            VerifyRoundTrip(CLValue.I64(-16),                               CLType.I64);
            VerifyRoundTrip(CLValue.U8(0x7F),                               CLType.U8);
            VerifyRoundTrip(CLValue.U32(uint.MaxValue),                     CLType.U32);
            VerifyRoundTrip(CLValue.U64(ulong.MaxValue),                    CLType.U64);
            VerifyRoundTrip(CLValue.U128(new BigInteger(ulong.MaxValue)),   CLType.U128);
            VerifyRoundTrip(CLValue.U256(new BigInteger(ulong.MaxValue)),   CLType.U256);
            VerifyRoundTrip(CLValue.U512(new BigInteger(ulong.MaxValue)),   CLType.U512);
            VerifyRoundTrip(CLValue.Unit(),                                  CLType.Unit);
            VerifyRoundTrip(CLValue.String("Hello, Casper!"),               CLType.String);
            VerifyRoundTrip(CLValue.URef("uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007"),
                            CLType.URef);
            VerifyRoundTrip(CLValue.List(new[] { CLValue.U32(1), CLValue.U32(2) }),
                            new CLListTypeInfo(CLType.U32));
            VerifyRoundTrip(CLValue.ByteArray(Hex.Decode("0102030405060708")),
                            new CLByteArrayTypeInfo(8));
            VerifyRoundTrip(CLValue.Ok(CLValue.U8(0xFF), new CLTypeInfo(CLType.String)),
                            new CLResultTypeInfo(CLType.U8, CLType.String));
            VerifyRoundTrip(CLValue.Tuple1(CLValue.U32(17)),                new CLTuple1TypeInfo(CLType.U32));
            VerifyRoundTrip(CLValue.Tuple2(CLValue.U32(17), CLValue.U32(127)),
                            new CLTuple2TypeInfo(CLType.U32, CLType.U32));
            VerifyRoundTrip(CLValue.Tuple3(CLValue.U32(17), CLValue.U32(127), CLValue.U32(17)),
                            new CLTuple3TypeInfo(CLType.U32, CLType.U32, CLType.U32));
        }
    }
}

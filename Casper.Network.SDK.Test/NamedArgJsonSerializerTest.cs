using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class NamedArgJsonSerializerTest
    {
        private NamedArgByteSerializer _byteSerializer;

        [SetUp]
        public void SetUp()
        {
            _byteSerializer = new NamedArgByteSerializer();
        }
        private string NAMEDARG_TARGET = @"[
          ""target"",
          {
            ""bytes"": ""989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"",
            ""cl_type"": {
              ""ByteArray"": 32
            },
            ""parsed"": ""989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7""
          }
        ]";
        
        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            WriteIndented = false,
            Converters = { new NamedArg.NamedArgConverter() }
        };
        
        [Test]
        public void DeserializeNamedArgTarget()
        {
            var namedArg = JsonSerializer.Deserialize<NamedArg>(NAMEDARG_TARGET, _options);
            Assert.IsNotNull(namedArg);
            Assert.AreEqual("target", namedArg.Name);
            
            var clValue = namedArg.Value;
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLByteArrayTypeInfo(32), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"), clValue.Bytes);
            Assert.AreEqual("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7", clValue.Parsed);
        }

        [Test]
        public void SerializeNamedArgTarget()
        {
            var clValue = CLValue.ByteArray("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7");
            var namedArg = new NamedArg("target", clValue);

            var strJson = JsonSerializer.Serialize(namedArg, _options);
            Assert.IsNotNull(strJson);

            var json = JsonDocument.Parse(strJson).RootElement;
            Assert.AreEqual(JsonValueKind.Array, json.ValueKind);
            Assert.AreEqual(2, json.EnumerateArray().Count());
            Assert.AreEqual("target", json.EnumerateArray().First().GetString());
            Assert.AreEqual(JsonValueKind.Object, json.EnumerateArray().Last().ValueKind);

            var clValue2 = JsonSerializer.Deserialize<CLValue>(json.EnumerateArray().Last().GetRawText());
            Assert.IsNotNull(clValue2);
        }

        [Test]
        public void FromBytes_Bool()
        {
            // "flag" + CLValue.Bool(true) => name(u32+bytes) + clvalue(u32 len + data + type tag)
            var original = new NamedArg("flag", CLValue.Bool(true));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("flag", result.Name);
            Assert.AreEqual(CLType.Bool, result.Value.TypeInfo.Type);
            Assert.AreEqual("01", Hex.ToHexString(result.Value.Bytes));
        }

        [Test]
        public void FromBytes_U32()
        {
            var original = new NamedArg("amount", CLValue.U32(uint.MaxValue));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("amount", result.Name);
            Assert.AreEqual(CLType.U32, result.Value.TypeInfo.Type);
            Assert.AreEqual("ffffffff", Hex.ToHexString(result.Value.Bytes));
        }

        [Test]
        public void FromBytes_U512()
        {
            var original = new NamedArg("payment", CLValue.U512(2_500_000_000UL));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("payment", result.Name);
            Assert.AreEqual(CLType.U512, result.Value.TypeInfo.Type);
        }

        [Test]
        public void FromBytes_String()
        {
            var original = new NamedArg("greeting", CLValue.String("Hello, Casper!"));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("greeting", result.Name);
            Assert.AreEqual(CLType.String, result.Value.TypeInfo.Type);
            Assert.AreEqual("Hello, Casper!", result.Value.ToString());
        }

        [Test]
        public void FromBytes_ByteArray()
        {
            var original = new NamedArg("target",
                CLValue.ByteArray("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("target", result.Name);
            Assert.AreEqual(CLType.ByteArray, result.Value.TypeInfo.Type);
            Assert.AreEqual(32, ((CLByteArrayTypeInfo)result.Value.TypeInfo).Size);
            Assert.AreEqual("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7",
                Hex.ToHexString(result.Value.Bytes));
        }

        [Test]
        public void FromBytes_URef()
        {
            var uref = "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007";
            var original = new NamedArg("purse", CLValue.URef(uref));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("purse", result.Name);
            Assert.AreEqual(CLType.URef, result.Value.TypeInfo.Type);
            Assert.AreEqual(33, result.Value.Bytes.Length);
        }

        [Test]
        public void FromBytes_OptionString()
        {
            var original = new NamedArg("maybe", CLValue.Option(CLValue.String("present")));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("maybe", result.Name);
            Assert.AreEqual(CLType.Option, result.Value.TypeInfo.Type);
            Assert.AreEqual(CLType.String, ((CLOptionTypeInfo)result.Value.TypeInfo).OptionType.Type);
        }

        [Test]
        public void FromBytes_Map()
        {
            var original = new NamedArg("data", CLValue.Map(new Dictionary<CLValue, CLValue>
            {
                { CLValue.String("key1"), CLValue.U32(1) },
                { CLValue.String("key2"), CLValue.U32(2) }
            }));
            var encoded = _byteSerializer.ToBytes(original);
            var result = _byteSerializer.FromBytes(encoded);

            Assert.AreEqual("data", result.Name);
            Assert.AreEqual(CLType.Map, result.Value.TypeInfo.Type);
            var mapType = (CLMapTypeInfo)result.Value.TypeInfo;
            Assert.AreEqual(CLType.String, mapType.KeyType.Type);
            Assert.AreEqual(CLType.U32, mapType.ValueType.Type);
        }

        [Test]
        public void FromBytes_RoundTrip()
        {
            // Verify ToBytes(FromBytes(ToBytes(x))) == ToBytes(x)
            var original = new NamedArg("value", CLValue.U64(ulong.MaxValue));
            var encoded = _byteSerializer.ToBytes(original);
            var decoded = _byteSerializer.FromBytes(encoded);
            var reEncoded = _byteSerializer.ToBytes(decoded);

            Assert.AreEqual(Hex.ToHexString(encoded), Hex.ToHexString(reEncoded));
        }

        [Test]
        public void FromBytes_List()
        {
            var bytes =
                "050000000a000000616d6f756e745f6f75740600000005ff54fceb06070d000000616d6f756e745f696e5f6d6178060000000500c846105e07040000007061746867000000030000000130f66fc8f60ee99e7a8a05c3a308ee623c229247c0cc7ac8dc76621157397a93018df5d26790e18cf0404502c62ce5dc9025800ad6975c97466e20506c39c505b601d65a7e04b687bd873af5819de67d563ff347560dfef16ad2c1065cac3e115b960e0b02000000746f21000000006dbf32d39ecc2c5936cf2ac132562e27630a9721156d249837f05348616709dc0b08000000646561646c696e6508000000ffffffffffffffff05";
            
            var namedArgs = NamedArg.ListFromBytes(Hex.Decode(bytes));
            Assert.AreEqual(5, namedArgs.Count);
            Assert.AreEqual("amount_out", namedArgs[0].Name);
            Assert.AreEqual(CLType.U256, namedArgs[0].Value.TypeInfo.Type);
            Assert.AreEqual(BigInteger.Parse("29728986367"), namedArgs[0].Value.ToBigInteger());
            Assert.AreEqual("amount_in_max", namedArgs[1].Name);
            Assert.AreEqual(CLType.U256, namedArgs[1].Value.TypeInfo.Type);
            Assert.AreEqual(BigInteger.Parse("404000000000"), namedArgs[1].Value.ToBigInteger());
            Assert.AreEqual("path", namedArgs[2].Name);
            Assert.AreEqual(CLType.List, namedArgs[2].Value.TypeInfo.Type);
            var list = namedArgs[2].Value.ToList<GlobalStateKey>();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("d65a7e04b687bd873af5819de67d563ff347560dfef16ad2c1065cac3e115b96", list[2].ToHexString());
            Assert.AreEqual("to", namedArgs[3].Name);
            Assert.AreEqual(CLType.Key, namedArgs[3].Value.TypeInfo.Type);
            Assert.AreEqual("account-hash-6dbf32d39ecc2c5936cf2ac132562e27630a9721156d249837f05348616709dc", 
                namedArgs[3].Value.ToString());
            Assert.AreEqual("deadline", namedArgs[4].Name);
            Assert.AreEqual(CLType.U64, namedArgs[4].Value.TypeInfo.Type);
            Assert.AreEqual(ulong.MaxValue, namedArgs[4].Value.ToUInt64());
        }
    }
}
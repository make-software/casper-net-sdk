using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class CLValueJsonDeserializerTest
    {
        private static string U512_JSON =
            "{\n" +
            "   \"cl_type\": \"U512\",\n" +
            "   \"bytes\": \"05005550b405\",\n" +
            "   \"parsed\": \"24500000000\" \n" +
            "}";

        private static string BYTEARRAY32_JSON =
            "{\n" +
            "   \"cl_type\": { \"ByteArray\": 32},\n" +
            "   \"bytes\": \"989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7\",\n" +
            "   \"parsed\": \"989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7\" \n" +
            "}";

        private static string OPTIONU64_JSON =
            "{\n" +
            "   \"cl_type\": { \"Option\": \"U64\"},\n" +
            "   \"bytes\": \"0179df0d8648700000\",\n" +
            "   \"parsed\": \"123456789012345\" \n" +
            "}";

        private static string OPTIONU64_NULL_JSON =
            "{\n" +
            "   \"cl_type\": { \"Option\": \"U64\"},\n" +
            "   \"bytes\": \"00\",\n" +
            "   \"parsed\": null\n" +
            "}";

        private static string OPTIONLISTU64_JSON =
            "{\n" +
            "   \"cl_type\": { \"Option\": { \"List\": \"U8\" } },\n" +
            "   \"bytes\": \"010400000010203040\",\n" +
            "   \"parsed\": [ 16, 32, 48, 64 ] \n" +
            "}";

        private static string LISTOPTIONSTRING_JSON =
            @"{
                ""cl_type"": {
                        ""List"": {
                            ""Option"": ""String""
                        }
                    },
                ""bytes"": ""040000000107000000537472696e67310107000000537472696e67320107000000537472696e67330107000000537472696e6734"",
                ""parsed"": [
                    ""String1"",
                    ""String2"",
                    ""String3"",
                    ""String4""
                ]
            }";

        private static string UREF_JSON =
            "{\n" +
            "    \"bytes\": \"000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f07\",\n" +
            "    \"cl_type\": \"URef\",\n" +
            "    \"parsed\": \"uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007\"\n" +
            "}";

        private static string KEY_ACCOUNT_JSON =
            @"{""cl_type"":""Key"",""bytes"":""00989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"",""parsed"":{""Account"":""account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7""}}";

        private static string KEY_UREF_JSON =
            @"{""cl_type"":""Key"",""bytes"":""02e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e407"",""parsed"":{""URef"":""uref-e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e4-007""}}";

        private static string KEY_ERAINFO_JSON =
            @"{""cl_type"":""Key"",""bytes"":""057d0a000000000000"",""parsed"":{""EraInfo"":""era-2685""}}";
            
        private static string TUPLE1_JSON =
            @"{
                ""cl_type"": {
                    ""Tuple1"": [
                        { ""Option"": ""String"" }
                    ]
                },
                ""bytes"": ""010400000061626364"",
                ""parsed"": [ ""abcd"" ]
            }";

        private static string TUPLE3_JSON =
            @"{
                ""cl_type"": {
                    ""Tuple3"": [
                        ""PublicKey"",
                        {
                            ""Option"": ""String""
                        },
                        ""U512""
                    ]
                },
                ""bytes"": ""02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e010400000061626364020008"",
                ""parsed"": [
                    ""02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e"",
                    ""abcd"",
                    ""2048""
                ]
            }";

        [Test]
        public void SerializeBoolCLValue()
        {
            var clValue = CLValue.Bool(true);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.Bool), clValue.TypeInfo);
            Assert.AreEqual(true, (bool)clValue.Parsed);
            var json = JsonSerializer.Serialize(clValue).Replace(" ", "");
            Assert.IsTrue(json.Contains(@"""cl_type"":""Bool"""));;
            Assert.IsTrue(json.Contains(@"""bytes"":""01"""));
            Assert.IsTrue(json.Contains(@"""parsed"":true"));
            
            clValue = CLValue.Bool(false);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.Bool), clValue.TypeInfo);
            Assert.AreEqual(false, (bool)clValue.Parsed);
            json = JsonSerializer.Serialize(clValue).Replace(" ", "");
            Assert.IsTrue(json.Contains(@"""bytes"":""00"""));
        }
        
        [Test]
        public void DeserializeBoolCLValue()
        {
            var json = @"{ ""cl_type"": ""Bool"", ""bytes"": ""01"", ""parsed"": true }";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.Bool), clValue.TypeInfo);
            Assert.AreEqual(true, clValue.Parsed);
        }

        [Test]
        public void DeserializeI32CLValue()
        {
            var json = @"{""cl_type"":""I32"",""bytes"":""00000080"",""parsed"":-2147483648}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(int.MinValue, BitConverterExtensions.ToInt32(clValue.Bytes));
            Assert.AreEqual(int.MinValue, clValue.Parsed);
        }
        
        [Test]
        public void SerializeU64CLValue()
        {
            var clValue = CLValue.U64(123456789012345);
            Assert.AreEqual(Hex.Decode("79df0d8648700000"), clValue.Bytes);
            Assert.AreEqual(new CLTypeInfo(CLType.U64), clValue.TypeInfo);
            Assert.AreEqual(123456789012345, (UInt64) clValue.Parsed);
        }
        
        [Test]
        public void DeserializeU64CLValue()
        {
            var json = @"{""cl_type"":""U64"",""bytes"":""ffffffffffffffff"",""parsed"":18446744073709551615}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(CLType.U64, clValue.TypeInfo.Type);
            Assert.AreEqual(ulong.MaxValue, BitConverterExtensions.ToUInt64(clValue.Bytes));
            Assert.AreEqual(ulong.MaxValue, clValue.Parsed);
        }

        [Test]
        public void DeserializeU512CLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(U512_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.U512), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("05005550b405"), clValue.Bytes);
            Assert.AreEqual("24500000000", clValue.Parsed);
        }

        [Test]
        public void SerializeU512CLValue()
        {
            CLValue clValue = new CLValue("05005550b405", new CLTypeInfo(CLType.U512), "24500000000");
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsNotNull(json);

            var doc = System.Text.Json.JsonDocument.Parse(json);
            Assert.AreEqual(3, doc.RootElement.EnumerateObject().Count());
            Assert.AreEqual("U512", doc.RootElement.GetProperty("cl_type").GetString());
            Assert.AreEqual("05005550b405", doc.RootElement.GetProperty("bytes").GetString().ToLower());
            Assert.AreEqual("24500000000", doc.RootElement.GetProperty("parsed").GetString());

            clValue = CLValue.U512(15000000000);
            json = JsonSerializer.Serialize(clValue).ToLower();
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"0500d6117e03\""));
            Assert.IsTrue(json.Contains("\"parsed\":\"15000000000\""));

            clValue = CLValue.U512(BigInteger.Parse("15000000000"));
            json = JsonSerializer.Serialize(clValue).ToLower();
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"0500d6117e03\""));
            Assert.IsTrue(json.Contains("\"parsed\":\"15000000000\""));

            clValue = CLValue.U512(0);
            json = JsonSerializer.Serialize(clValue).ToLower();
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"bytes\":\"00\""));
            Assert.IsTrue(json.Contains("\"parsed\":\"0\""));
        }

        [Test]
        public void DeserializeByteArray32CLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(BYTEARRAY32_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLByteArrayTypeInfo(32), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"),
                clValue.Bytes);
            Assert.AreEqual("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7", clValue.Parsed);
        }

        [Test]
        public void SerializeByteArray32CLValue()
        {
            var clValue = new CLValue("989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7",
                new CLByteArrayTypeInfo(32), "989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7");

            var json = JsonSerializer.Serialize(clValue);
            Assert.IsNotNull(json);
        }

        [Test]
        public void DeserializeOptionU64CLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(OPTIONU64_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLOptionTypeInfo(new CLTypeInfo(CLType.U64)), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("0179df0d8648700000"), clValue.Bytes);
            Assert.AreEqual("123456789012345", clValue.Parsed);

            clValue = JsonSerializer.Deserialize<CLValue>(OPTIONU64_NULL_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLOptionTypeInfo(new CLTypeInfo(CLType.U64)), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("00"), clValue.Bytes);
            Assert.IsNull(clValue.Parsed);

            clValue = JsonSerializer.Deserialize<CLValue>(OPTIONLISTU64_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLOptionTypeInfo(new CLListTypeInfo(new CLTypeInfo(CLType.U8))), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("010400000010203040"), clValue.Bytes);
        }

        [Test]
        public void SerializeOptionU64CLValue()
        {
            var clValue = CLValue.U64(123456789012345);
            var optionClValue = CLValue.Option(clValue);

            var json = JsonSerializer.Serialize(optionClValue);
            Assert.IsNotNull(json);
        }

        [Test]
        public void SerializeStringCLValue()
        {
            var clValue = CLValue.String("Hello, World!");
            Assert.IsNotNull(clValue);
            Assert.AreEqual(Hex.Decode("0d00000048656c6c6f2c20576f726c6421"), clValue.Bytes);
            Assert.AreEqual(new CLTypeInfo(CLType.String), clValue.TypeInfo);
            Assert.AreEqual("Hello, World!", clValue.Parsed);
        }

        [Test]
        public void SerializeUnitCLValue()
        {
            var clValue = CLValue.Unit();
            Assert.IsNotNull(clValue);
            Assert.AreEqual(Array.Empty<byte>(), clValue.Bytes);
            Assert.IsNull(clValue.Parsed);

            var json = JsonSerializer.Serialize(clValue);
        }

        [Test]
        public void SerializeUrefCLValue()
        {
            var clValue = new CLValue(Hex.Decode("000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f07"),
                new CLTypeInfo(CLType.URef),
                "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007");
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsNotNull(json);
        }

        [Test]
        public void DeserializeURefCLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(UREF_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.URef), clValue.TypeInfo);
            Assert.AreEqual(Hex.Decode("000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f07"),
                clValue.Bytes);
            Assert.AreEqual("uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007",
                clValue.Parsed);
        }

        [Test]
        public void SerializeListCLValue()
        {
            var list = new CLValue[] {CLValue.String("AAAA"), CLValue.String("BBBBB")};
            var clValue = CLValue.List(list);
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""020000000400000041414141050000004242424242"""));
            Assert.IsTrue(json.Contains(@"""List"":""String"""));
        }

        [Test]
        public void DeserializeListCLValue()
        {
            var json = "{\"cl_type\":{\"List\":\"String\"}," +
                       "\"bytes\":\"020000000400000041414141050000004242424242\",\"parsed\":\"\"}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLListTypeInfo(new CLTypeInfo(CLType.String)), clValue.TypeInfo);
            Assert.AreEqual("020000000400000041414141050000004242424242", Hex.ToHexString(clValue.Bytes));
            Assert.AreEqual("", clValue.Parsed);
        }

        [Test]
        public void DeserializeComplexListCLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(LISTOPTIONSTRING_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLListTypeInfo(new CLOptionTypeInfo(new CLTypeInfo(CLType.String))), clValue.TypeInfo);
            Assert.AreEqual(
                "040000000107000000537472696e67310107000000537472696e67320107000000537472696e67330107000000537472696e6734",
                Hex.ToHexString(clValue.Bytes));
        }

        [Test]
        public void SerializeTuple1CLValue()
        {
            var clValue = CLValue.Tuple1(CLValue.U32(17));
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""11000000"""));

            clValue = CLValue.Tuple1(CLValue.String("AAAA"));
            json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""0400000041414141"""));
        }

        [Test]
        public void DeserializeTuple1CLValue()
        {
            var json = @"{ ""cl_type"": { ""Tuple1"": [""U32""] }, ""bytes"": ""11000000"", ""parsed"": ""null"" }";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTuple1TypeInfo(new CLTypeInfo(CLType.U32)), clValue.TypeInfo);
            Assert.AreEqual("11000000", Hex.ToHexString(clValue.Bytes));
            Assert.AreEqual("null", clValue.Parsed);

            clValue = JsonSerializer.Deserialize<CLValue>(TUPLE1_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTuple1TypeInfo(new CLOptionTypeInfo(new CLTypeInfo(CLType.String))),
                clValue.TypeInfo);
            Assert.AreEqual("010400000061626364", Hex.ToHexString(clValue.Bytes));
        }

        [Test]
        public void SerializeTuple2CLValue()
        {
            var clValue = CLValue.Tuple2(CLValue.U32(17), CLValue.U32(255));
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""11000000ff000000"""));

            clValue = CLValue.Tuple2(CLValue.U32(255), CLValue.String("AAAA"));
            json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""ff0000000400000041414141"""));
        }

        [Test]
        public void DeserializeTuple2CLValue()
        {
            var json =
                @"{ ""bytes"": ""ff0000000400000041414141"", ""parsed"": ""null"", ""cl_type"": { ""Tuple2"": [""U32"", ""String""] } }";
            //var json = @"{ ""cl_type"": { ""Tuple2"": [""U32"", ""String""] } }";

            Console.WriteLine(json);
            var doc = JsonDocument.Parse(json);
            var text = doc.ToString();
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTuple2TypeInfo(new CLTypeInfo(CLType.U32), new CLTypeInfo(CLType.String)),
                clValue.TypeInfo);
            Assert.AreEqual("ff0000000400000041414141", Hex.ToHexString(clValue.Bytes));
            Assert.AreEqual("null", clValue.Parsed);
        }

        [Test]
        public void SerializeTuple3CLValue()
        {
            var clValue = CLValue.Tuple3(CLValue.U32(17), CLValue.String("AAAA"), CLValue.U32(127));
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.Contains(@"""1100000004000000414141417f000000"""));
        }

        [Test]
        public void DeserializeTuple3CLValue()
        {
            var json =
                @"{ ""cl_type"": { ""Tuple3"": [""U32"", ""String"", ""U32""] }, ""bytes"": ""1100000004000000414141417f000000"", ""parsed"": ""null"" }";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(
                new CLTuple3TypeInfo(new CLTypeInfo(CLType.U32), new CLTypeInfo(CLType.String),
                    new CLTypeInfo(CLType.U32)), clValue.TypeInfo);
            Assert.AreEqual("1100000004000000414141417f000000", Hex.ToHexString(clValue.Bytes));
            Assert.AreEqual("null", clValue.Parsed);
        }

        [Test]
        public void DeserializeComplexTuple3CLValue()
        {
            var clValue = JsonSerializer.Deserialize<CLValue>(TUPLE3_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(
                new CLTuple3TypeInfo(new CLTypeInfo(CLType.PublicKey),
                    new CLOptionTypeInfo(new CLTypeInfo(CLType.String)),
                    new CLTypeInfo(CLType.U512)), clValue.TypeInfo);
            Assert.AreEqual(
                "02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e010400000061626364020008",
                Hex.ToHexString(clValue.Bytes));
        }

        [Test]
        public void SerializePublicKeyCLValue()
        {
            var pk = PublicKey.FromHexString("01381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb771");
            var clValue = CLValue.PublicKey(pk);
            var json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.ToLower().Contains(@"""01381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb771"""));
            Assert.IsTrue(json.Contains(@"""cl_type"":""PublicKey"""));

            pk = PublicKey.FromHexString("02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e");
            clValue = CLValue.PublicKey(pk);
            json = JsonSerializer.Serialize(clValue);
            Assert.IsTrue(json.ToLower().Contains(@"""02037292af42f13f1f49507c44afe216b37013e79a062d7e62890f77b8adad60501e"""));
            Assert.IsTrue(json.Contains(@"""cl_type"":""PublicKey"""));
        }

        [Test]
        public void DeserializePublicKeyCLValue()
        {
            var json = "{" +
                       @"""cl_type"": ""PublicKey""," +
                       @"""bytes"": ""01381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb771""," +
                       @"""parsed"": ""null""" +
                       "}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLTypeInfo(CLType.PublicKey), clValue.TypeInfo);
            Assert.AreEqual("01381b36cd07ad85348607ffe0fa3a2d033ea941d14763358ebeace9c8ad3cb771",
                Hex.ToHexString(clValue.Bytes));
            Assert.AreEqual("null", clValue.Parsed);
        }

        [Test]
        public void SerializeResultOkCLValue()
        {
            var clValue = CLValue.Ok(CLValue.U32(0xFFFEFDFC), new CLTypeInfo(CLType.String));
            var json = JsonSerializer.Serialize(clValue);
            json = json.Replace(" ", "");
            Assert.IsTrue(json.Contains(@"{""Result"":{""ok"":""U32"",""err"":""String""}"));
            Assert.IsTrue(json.ToLower().Contains(@"""bytes"":""01fcfdfeff"""));
        }
        
        [Test]
        public void SerializeResultErrCLValue()
        {
            var clValue = CLValue.Err(CLValue.String("Failure"), new CLTypeInfo(CLType.U32));
            var json = JsonSerializer.Serialize(clValue);
            json = json.Replace(" ", "");
            Assert.IsTrue(json.Contains(@"{""Result"":{""ok"":""U32"",""err"":""String""}"));
            Assert.IsTrue(json.ToLower().Contains(@"""bytes"":""00070000004661696c757265"""));
        }
        
        [Test]
        public void DeserializeResultOkCLValue()
        {
            var json =
                @"{""cl_type"":{""Result"":{""ok"":""String"",""err"":""String""}},""bytes"":""010700000053756363657373"",""parsed"":{""Ok"":""Success""}}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLResultTypeInfo(new CLTypeInfo(CLType.String), new CLTypeInfo(CLType.String)),
                clValue.TypeInfo);
            Assert.AreEqual("010700000053756363657373", Hex.ToHexString(clValue.Bytes));
            Assert.IsNotNull(clValue.Parsed);
        }

        [Test]
        public void DeserializeResultErrCLValue()
        {
            var json =
                @"{""cl_type"":{""Result"":{""ok"":""String"",""err"":""String""}},""bytes"":""00070000004661696c757265"",""parsed"":{""Err"":""Failure""}}";
            var clValue = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(new CLResultTypeInfo(new CLTypeInfo(CLType.String), new CLTypeInfo(CLType.String)),
                clValue.TypeInfo);
            Assert.AreEqual("00070000004661696c757265", Hex.ToHexString(clValue.Bytes));
            Assert.IsNotNull(clValue.Parsed);
        }

        [Test]
        public void SerializeMapCLValue()
        {
            var dict = new Dictionary<CLValue, CLValue>()
            {
                {CLValue.String("fourteen"), CLValue.Option(CLValue.String("14"))},
                {CLValue.String("fifteen"), CLValue.Option(CLValue.String("15"))},
                {CLValue.String("sixteen"), CLValue.Option(CLValue.String("16"))}
            };
            
            var map = CLValue.Map(dict);
            var json = JsonSerializer.Serialize(map).Replace(" ", "");
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains(@"""cl_type"":{""Map"":{""key"":""String"",""value"":{""Option"":""String""}}}"));
            Assert.IsTrue(json.ToLower().Contains(@"""bytes"":""0300000008000000666f75727465656e01020000003134070000006669667465656e01020000003135070000007369787465656e01020000003136"""));
        }

        [Test]
        public void SerializeKeyCLValue()
        {
            var gsKey = GlobalStateKey.FromString(
                "withdraw-b19223deb110bdd24316f6a3fc6bd99d0ad918aef18167a595b3cb622a410ec1");
            var clValue = CLValue.Key(gsKey);
            var json = JsonSerializer.Serialize(clValue).Replace(" ", "");
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains(@"""cl_type"":""Key"""));
            Assert.IsTrue(json.Contains(@"""bytes"":""08b192"));
            Assert.IsTrue(json.Contains(@"""parsed"":{""Withdraw"":""withdraw-b192"));

            gsKey = GlobalStateKey.FromString(
                "uref-e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e4-007");
            clValue = CLValue.Key(gsKey);
            json = JsonSerializer.Serialize(clValue).Replace(" ", "");
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains(@"""cl_type"":""Key"""));
            Assert.IsTrue(
                json.Contains(@"""bytes"":""02e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e407"));
            Assert.IsTrue(json.Contains(
                @"""parsed"":{""URef"":""uref-e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e4-007"));

            gsKey = GlobalStateKey.FromString("era-2685");
            clValue = CLValue.Key(gsKey);
            json = JsonSerializer.Serialize(clValue).Replace(" ", "");
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains(@"""cl_type"":""Key"""));
            Assert.IsTrue(json.ToLower().Contains(@"""bytes"":""057d0a000000000000"));
            Assert.IsTrue(json.Contains(@"""parsed"":{""EraInfo"":""era-2685""}"));
        }

        [Test]
        public void DeserializeKeyCLValue()
        {

            var clValue = JsonSerializer.Deserialize<CLValue>(KEY_ACCOUNT_JSON);
            
            Assert.IsNotNull(clValue);
            Assert.AreEqual(CLType.Key, clValue.TypeInfo.Type);
            Assert.IsTrue(Hex.Decode("00989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7").SequenceEqual(clValue.Bytes));
            if(clValue.Parsed is JsonElement jAccount)
                Assert.AreEqual(@"{""Account"":""account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7""}", 
                    jAccount.GetRawText().Replace(" ",""));

            clValue = JsonSerializer.Deserialize<CLValue>(KEY_UREF_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(CLType.Key, clValue.TypeInfo.Type);
            Assert.IsTrue(Hex.Decode("02e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e407").SequenceEqual(clValue.Bytes));
            if(clValue.Parsed is JsonElement jURef)
                Assert.AreEqual(@"{""URef"":""uref-e48935c79e96c490c01e1e8800de5ec5f4a857a57db0dcffed1e1e2b5d29b5e4-007""}", 
                    jURef.GetRawText().Replace(" ",""));
            
            clValue = JsonSerializer.Deserialize<CLValue>(KEY_ERAINFO_JSON);
            Assert.IsNotNull(clValue);
            Assert.AreEqual(CLType.Key, clValue.TypeInfo.Type);
            Assert.IsTrue(Hex.Decode("057d0a000000000000").SequenceEqual(clValue.Bytes));
            if(clValue.Parsed is JsonElement jEraInfo)
                Assert.AreEqual(@"{""EraInfo"":""era-2685""}", 
                    jEraInfo.GetRawText().Replace(" ",""));
        }

    }
}
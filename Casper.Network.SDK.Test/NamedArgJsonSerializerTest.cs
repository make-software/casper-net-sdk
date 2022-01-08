using System.Linq;
using System.Text.Json;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class NamedArgJsonSerializerTest
    {
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
    }
}
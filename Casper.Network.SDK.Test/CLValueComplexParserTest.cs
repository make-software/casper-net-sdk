using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NetCasperTest
{
    public class CLValueComplexParserTest
    {
        [Test]
        public void ListOptionStringDeserializer()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/list-option-string.json";
            var json = File.ReadAllText(testFile);

            var value = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(value);
            var list = value.ToList();
            Assert.IsNotNull(list);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("String1", list[0]);
            Assert.AreEqual("String2", list[1]);
            Assert.IsNull(list[2]);
            Assert.AreEqual("String4", list[3]);

            var list2 = value.ToList<string>();
            Assert.IsNotNull(list2);
            Assert.AreEqual(4, list2.Count);
            Assert.AreEqual("String1", list2[0]);
            Assert.AreEqual("String2", list2[1]);
            Assert.IsNull(list2[2]);
            Assert.AreEqual("String4", list2[3]);
        }
        
        [Test]
        public void ResultUnitStringDeserializer()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/result-unit-string.json";
            var json = File.ReadAllText(testFile);

            var value = JsonSerializer.Deserialize<CLValue>(json);
            Assert.IsNotNull(value);
        }

        [Test]
        public void ComplexMapDeserializer()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/map-complex.json";
            var json = File.ReadAllText(testFile);

            var value = JsonSerializer.Deserialize<CLValue>(json);
            var dict = value.ToDictionary();
            Assert.IsNotNull(value);
            Assert.AreEqual(2, dict.Count);

            var tuple = dict["map3key1"] as ITuple;
            Assert.IsNotNull(tuple);
            Assert.AreEqual(3, tuple.Length);
            Assert.AreEqual(11, tuple[0]);
            Assert.AreEqual(true, tuple[2]);

            var dict2 = tuple[1] as IDictionary;
            Assert.IsNotNull(dict2);
            Assert.AreEqual(2, dict2.Count);

            var tuple2 = dict2["map2key2"] as ITuple;
            Assert.IsNotNull(tuple2);
            Assert.AreEqual(3, tuple2.Length);
            Assert.AreEqual(2, tuple2[0]);
            Assert.AreEqual(false, tuple2[1]);

            var dict3 = tuple2[2] as IDictionary;
            Assert.IsNotNull(dict3);
            Assert.AreEqual(2, dict3.Count);

            var tuple3 = dict3["first"] as ITuple;
            Assert.IsNotNull(tuple3);
            Assert.AreEqual(3, tuple3.Length);
            Assert.AreEqual(123, tuple3[0]);
            Assert.AreEqual(true, tuple3[1]);

            var uref = tuple3[2] as URef;
            Assert.IsNotNull(uref);
            Assert.AreEqual("uref-cdd5422295f6a61e86a4d3229b28dac2e67523c41e2aafed3a041362df7a8432-007",
                uref.ToString().ToLower());
        }
    }
}
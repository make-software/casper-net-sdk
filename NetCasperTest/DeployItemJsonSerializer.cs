using System;
using System.Numerics;
using System.Text.Json;
using NetCasperSDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class DeployItemJsonSerializer
    {
        [Test]
        public void ModuleBytesByteSerialization()
        {
            var bigInt1000 = new BigInteger(1000);
            var moduleBytes = new ModuleBytesDeployItem(bigInt1000);

            var jsonModBytes = JsonSerializer.Serialize(moduleBytes);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(jsonModBytes));

            var deserialized = JsonSerializer.Deserialize<ModuleBytesDeployItem>(jsonModBytes);
            
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(Array.Empty<byte>(), deserialized.ModuleBytes);
            var deserializedRuntimeArg = deserialized.RuntimeArgs[0];
            Assert.AreEqual(1000, deserializedRuntimeArg);
        }
    }
}
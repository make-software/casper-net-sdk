using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class TransformsTest
    {
        [Test]
        public void TransformsTest_v156()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/deploy-testing-effects-v156.json");
            
            var result = RpcResult.Parse<GetDeployResult>(json);
            Assert.IsNotNull(result);

            var effects = result.ExecutionInfo.ExecutionResult.Effect;
            Assert.AreEqual(2, effects.Count(t => t.Kind is IdentityTransformKind));

            Assert.AreEqual(4, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.CLValue is not null));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.CLValue is not null &&
                                                  kind.Value.CLValue.TypeInfo.Type == CLType.String &&
                                                  kind.Value.CLValue.ToString().Equals("JJJ")));

            var addU64Kind =
                effects.FirstOrDefault(t => t.Kind is AddUInt64TransformKind).Kind as AddUInt64TransformKind;
            Assert.AreEqual(555, addU64Kind.Value); 
            var addU512Kind =
                effects.FirstOrDefault(t => t.Kind is AddUInt512TransformKind).Kind as AddUInt512TransformKind;
            Assert.AreEqual(new BigInteger(1000000000000), addU512Kind.Value);

            var addKeysKind = 
                effects.FirstOrDefault(t => t.Kind is AddKeysTransformKind).Kind as AddKeysTransformKind;
            Assert.AreEqual(1, addKeysKind.Value.Count);
            Assert.AreEqual("cep18_contract_package_JJJ", addKeysKind.Value[0].Name);
            Assert.AreEqual("hash-0505050505050505050505050505050505050505050505050505050505050505", addKeysKind.Value[0].Key.ToString());
            
            Assert.AreEqual(2, effects.Count(t => t.Kind is WriteContractPackageLegacyTransformKind));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteContractPackageLegacyTransformKind &&
                                                  t.Key.ToString().Equals("hash-0505050505050505050505050505050505050505050505050505050505050505")));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteContractLegacyTransformKind));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteContractWasmLegacyTransformKind));
            
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.DeployInfo is not null));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.Transfer is not null));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.Bid is not null));
            Assert.AreEqual(1, effects.Count(t => t.Kind is WriteTransformKind kind &&
                                                  kind.Value.Unbonding is not null));
        }

        [Test]
        public void WriteContractTransform_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/write_contract_transform_v2.json");
            
            var  options = new JsonSerializerOptions()
            {
                WriteIndented = false,
                Converters =
                {
                    new Transform.TransformConverter(),
                    new StoredValue.StoredValueConverter(),
                }
            };
            var transform = JsonSerializer.Deserialize<Transform>(json, options);

            Assert.IsNotNull(transform);
            Assert.IsNotNull(transform.Key);
            var writeTransform = transform.Kind as WriteTransformKind;
            Assert.IsNotNull(writeTransform);
            Assert.IsNotNull(writeTransform.Value);
            var contract = writeTransform.Value.Contract;
            Assert.IsNotNull(contract);
            Assert.IsTrue(contract.EntryPoints.Count > 0);
        }
    }
}
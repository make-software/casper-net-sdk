using System.IO;
using System.Text.Json;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class DeployJsonSerializer
    {
        private string deploy1 = TestContext.CurrentContext.TestDirectory + "/TestData/transfer-deploy.json";
        
        [Test]
        public void LoadDeployFromFileTest()
        {
            var deploy = Deploy.Load(deploy1);

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new PublicKey.PublicKeyConverter(),
                    new ExecutableDeployItemConverter()
                }
            };
            var json = JsonSerializer.Serialize(deploy);
            File.WriteAllText("/Users/davidhernando/Documents/Casper/serialized_deploy.json", json);
            
            Assert.IsNotNull(deploy);
            Assert.IsTrue(deploy.ValidateHashes(out string msg));
            Assert.AreEqual(1, deploy.Approvals.Count);
            Assert.AreEqual("017b8058863aad49c7b89c77019cef3a4d863bdf1c0c61499776f94b18465810f7", 
                deploy.Approvals[0].Signer.ToAccountHex());
            Assert.AreEqual("012a0c5896ab9d6cf029268cf454ba9d42bcd40bd909c984890f326739eef13201d1d2a9a95938b2a966f2e650d1bdd80931f0374e3a92403e025b806aa1065109", 
                deploy.Approvals[0].Signature.ToHexString());
        }

        [Test]
        public void ParseDeployFromJsonString()
        {
            var json = File.ReadAllText(deploy1);
            var deploy = Deploy.Parse(json);
            
            Assert.IsNotNull(deploy);
            Assert.IsTrue(deploy.ValidateHashes(out string msg));
            Assert.AreEqual(1, deploy.Approvals.Count);
            Assert.AreEqual("017b8058863aad49c7b89c77019cef3a4d863bdf1c0c61499776f94b18465810f7", 
                deploy.Approvals[0].Signer.ToAccountHex());
            Assert.AreEqual("012a0c5896ab9d6cf029268cf454ba9d42bcd40bd909c984890f326739eef13201d1d2a9a95938b2a966f2e650d1bdd80931f0374e3a92403e025b806aa1065109", 
                deploy.Approvals[0].Signature.ToHexString());
        }

        [Test]
        public void SaveDeployToFile()
        {
            var deploy = Deploy.Load(deploy1);

            var tmpFile = Path.GetTempFileName();
            deploy.Save(tmpFile);

            var deployCopy = Deploy.Load(tmpFile);
            Assert.IsTrue(deployCopy.ValidateHashes(out string message));
        }

        [Test]
        public void SerializeDeployToJson()
        {
            var deploy = Deploy.Load(deploy1);

            var json = deploy.SerializeToJson();

            var deployCopy = Deploy.Parse(json);
            Assert.IsTrue(deployCopy.ValidateHashes(out string message));
        }
    }
}
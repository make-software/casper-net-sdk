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
        private string signer1 = "017b8058863Aad49c7b89c77019ceF3a4D863BDf1c0c61499776F94b18465810F7";
        private string signature1 =
            "012a0c5896ab9d6cf029268cf454ba9d42bcd40bd909c984890f326739eef13201d1d2a9a95938b2a966f2e650d1bdd80931f0374e3a92403e025b806aa1065109";
        
        [Test]
        public void LoadDeployFromFileTest()
        {
            var deploy = Deploy.Load(deploy1);

            Assert.IsNotNull(deploy);
            Assert.IsTrue(deploy.ValidateHashes(out string msg));
            Assert.AreEqual(string.Empty, msg);
            Assert.AreEqual(1, deploy.Approvals.Count);
            Assert.AreEqual(signer1, deploy.Approvals[0].Signer.ToAccountHex());
            Assert.AreEqual(signature1, deploy.Approvals[0].Signature.ToHexString());
        }

        [Test]
        public void ParseDeployFromJsonString()
        {
            var json = File.ReadAllText(deploy1);
            var deploy = Deploy.Parse(json);
            
            Assert.IsNotNull(deploy);
            Assert.IsTrue(deploy.ValidateHashes(out string msg));
            Assert.AreEqual(string.Empty, msg);
            Assert.AreEqual(1, deploy.Approvals.Count);
            Assert.AreEqual(signer1, deploy.Approvals[0].Signer.ToAccountHex());
            Assert.AreEqual(signature1, deploy.Approvals[0].Signature.ToHexString());
        }

        [Test]
        public void SaveDeployToFile()
        {
            var deploy = Deploy.Load(deploy1);

            var tmpFile = Path.GetTempFileName();
            deploy.Save(tmpFile);

            var deployCopy = Deploy.Load(tmpFile);
            Assert.IsTrue(deployCopy.ValidateHashes(out string msg));
            Assert.AreEqual(string.Empty, msg);
        }

        [Test]
        public void SerializeDeployToJson()
        {
            var deploy = Deploy.Load(deploy1);

            var json = deploy.SerializeToJson();

            var deployCopy = Deploy.Parse(json);
            Assert.IsTrue(deployCopy.ValidateHashes(out string msg));
            Assert.AreEqual(string.Empty, msg);
        }
    }
}
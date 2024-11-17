using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class GetPackageResultTest
    {
        [Test]
        public void GetPackageResultContractPackageTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-package-result-contract-package-v200.json");

            var result = RpcResult.Parse<GetPackageResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.IsNotNull(result.ContractPackage);
            Assert.IsNull(result.Package);
            Assert.AreEqual("uref-6fc684fea74b278cbb18b546a6d9242b810ce58a2ff05d17493b19aa08f540e0-007", result.ContractPackage.AccessKey.ToString());
            Assert.AreEqual(1, result.ContractPackage.Versions.Count);
            Assert.AreEqual(2, result.ContractPackage.Versions[0].ProtocolVersionMajor);
            Assert.AreEqual(1, result.ContractPackage.Versions[0].Version);
            Assert.AreEqual("contract-25aa2d3cc62a302746c08ae885454d6e8a9c8609aaa7468b24284e5d29c5d2f1", result.ContractPackage.Versions[0].Hash);
            Assert.AreEqual(LockStatus.Unlocked, result.ContractPackage.LockStatus);
        }
        [Test]
        public void GetPackageResultPackageTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-package-result-package-v200.json");

            var result = RpcResult.Parse<GetPackageResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.IsNull(result.ContractPackage);
            Assert.IsNotNull(result.Package);
            Assert.AreEqual(1, result.Package.Versions.Count);
            Assert.AreEqual(2, result.Package.Versions[0].EntityVersion.ProtocolVersionMajor);
            Assert.AreEqual(1, result.Package.Versions[0].EntityVersion.Version);
            Assert.AreEqual("addressable-entity-e51af99d88fd26a282de00271c49a6c256232b344aa7907d2c8603b2bd5217c9", result.Package.Versions[0].AddressableEntityHash);
            Assert.AreEqual(LockStatus.Unlocked, result.Package.LockStatus);
        }
    }
}
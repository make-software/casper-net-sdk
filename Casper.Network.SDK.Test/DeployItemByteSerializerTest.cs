using System;
using System.Numerics;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class ByteSerializerTest
    {
        private ExecutableDeployItemByteSerializer serializer;
        
        [SetUp]
        public void SetUp()
        {
            serializer = new ExecutableDeployItemByteSerializer();
        }

        [Test]
        public void ModuleBytesByteSerialization()
        {
            var bigInt1000 = new BigInteger(1000);
            var moduleBytes = new ModuleBytesDeployItem(bigInt1000);

            Assert.AreEqual(Array.Empty<byte>(), moduleBytes.ModuleBytes);
            Assert.IsNotNull(moduleBytes.RuntimeArgs);
            Assert.AreEqual(1, moduleBytes.RuntimeArgs.Count);

            byte[] bytes = serializer.ToBytes(moduleBytes);
            Assert.AreEqual("00000000000100000006000000616d6f756e740300000002e80308", Hex.ToHexString(bytes));
        }

        [Test]
        public void StoredContractByHashSerialization()
        {
            var hash = new HashKey("hash-0102030401020304010203040102030401020304010203040102030401020304");
            var storedContract =
                new StoredContractByHashDeployItem(hash.ToHexString(),
                    "counter_inc");
            
            Assert.AreEqual(Hex.ToHexString(hash.RawBytes), storedContract.Hash);
            Assert.AreEqual("counter_inc", storedContract.EntryPoint);
            Assert.IsNotNull(storedContract.RuntimeArgs);
            Assert.AreEqual(0, storedContract.RuntimeArgs.Count);
            
            byte[] bytes = serializer.ToBytes(storedContract);
            Assert.AreEqual("0101020304010203040102030401020304010203040102030401020304010203040b000000636f756e7465725f696e6300000000", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void StoredContractByNameSerialization()
        {
            var storedContract =
                new StoredContractByNameDeployItem("counter", "counter_inc");
            
            Assert.AreEqual("counter", storedContract.Name);
            Assert.AreEqual("counter_inc", storedContract.EntryPoint);
            Assert.IsNotNull(storedContract.RuntimeArgs);
            Assert.AreEqual(0, storedContract.RuntimeArgs.Count);
            
            byte[] bytes = serializer.ToBytes(storedContract);
            Assert.AreEqual("0207000000636f756e7465720b000000636f756e7465725f696e6300000000", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void StoredVersionedContractByHashSerialization()
        {
            var hash = new HashKey("hash-0102030401020304010203040102030401020304010203040102030401020304");
            var storedContract =
                new StoredVersionedContractByHashDeployItem(hash.ToHexString(), 1, "counter_inc");
            
            Assert.AreEqual(Hex.ToHexString(hash.RawBytes), storedContract.Hash);
            Assert.AreEqual(1, storedContract.Version);
            Assert.AreEqual("counter_inc", storedContract.EntryPoint);
            Assert.IsNotNull(storedContract.RuntimeArgs);
            Assert.AreEqual(0, storedContract.RuntimeArgs.Count);
            
            byte[] bytes = serializer.ToBytes(storedContract);
            Assert.AreEqual("03010203040102030401020304010203040102030401020304010203040102030401010000000b000000636f756e7465725f696e6300000000", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void StoredVersionedContractByNameSerialization()
        {
            var storedContract =
                new StoredVersionedContractByNameDeployItem("counter", 15, "counter_inc");
            
            Assert.AreEqual(15, storedContract.Version);
            Assert.AreEqual("counter", storedContract.Name);
            Assert.AreEqual("counter_inc", storedContract.EntryPoint);
            Assert.IsNotNull(storedContract.RuntimeArgs);
            Assert.AreEqual(0, storedContract.RuntimeArgs.Count);
            
            byte[] bytes = serializer.ToBytes(storedContract);
            Assert.AreEqual("0407000000636f756e746572010f0000000b000000636f756e7465725f696e6300000000", Hex.ToHexString(bytes));
        }
        
        [Test]
        public void TransferByteSerialization()
        {
            var tgtKey = PublicKey.FromHexString("01027c04a0210afdf4a83328d57e8c2a12247a86d872fb53367f22a84b1b53d2a9");
            var transfer = new TransferDeployItem(
                15000000000,
                new AccountHashKey(tgtKey),
                12345);

            Assert.AreEqual(3, transfer.RuntimeArgs.Count);
            
            var bytes = serializer.ToBytes(transfer);
            var hex = Hex.ToHexString(bytes);
            Assert.AreEqual("05" + "03000000" + "06000000616d6f756e74060000000500d6117e0308" +
                            "06000000746172676574200000007cfcb2fbdd0e747cabd0f8fe4d743179a764a8d7174ea6f0dfdb0c41fe1348b40f20000000" + 
                            "020000006964090000000139300000000000000d05", 
                hex);
        }
    }
}
using System.Collections.Generic;
using System.Text.Json;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.TransactionBuilder
{
    public class ProtocolVersionMajor
    {
        [Test]
        public void ByPackageHashNoVersionTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageHash("0101010101010101010101010101010101010101010101010101010101010101")
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var txv1 = (TransactionV1)transaction;
            var target = txv1.Payload.Target as StoredTransactionV1Target;
            var invocationTarget = target.Id as ByPackageHashInvocationTarget;
            Assert.AreEqual("0101010101010101010101010101010101010101010101010101010101010101", invocationTarget.Hash);
            Assert.AreEqual(null, invocationTarget.Version);
            Assert.AreEqual(null, invocationTarget.ProtocolVersionMajor);
        }
        
        [Test]
        public void ByPackageHashWithVersionTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageHash("0101010101010101010101010101010101010101010101010101010101010101", null, 2)
                // .ByPackageName("counter_package_name", 1, 1)
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var txv1 = (TransactionV1)transaction;
            var target = txv1.Payload.Target as StoredTransactionV1Target;
            var invocationTarget = target.Id as ByPackageHashInvocationTarget;
            Assert.AreEqual("0101010101010101010101010101010101010101010101010101010101010101", invocationTarget.Hash);
            Assert.AreEqual(null, invocationTarget.Version);
            Assert.AreEqual(2, invocationTarget.ProtocolVersionMajor);
        }
        
        [Test]
        public void ByPackageNameNoVersionTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageName("counter_package_name")
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var txv1 = (TransactionV1)transaction;
            var target = txv1.Payload.Target as StoredTransactionV1Target;
            var invocationTarget = target.Id as ByPackageNameInvocationTarget;
            Assert.AreEqual("counter_package_name", invocationTarget.Name);
            Assert.AreEqual(null, invocationTarget.Version);
            Assert.AreEqual(null, invocationTarget.ProtocolVersionMajor);
        }
        
        [Test]
        public void ByPackageNameWithVersionTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageName("counter_package_name", 1, 2)
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var txv1 = (TransactionV1)transaction;
            var target = txv1.Payload.Target as StoredTransactionV1Target;
            var invocationTarget = target.Id as ByPackageNameInvocationTarget;
            Assert.AreEqual("counter_package_name", invocationTarget.Name);
            Assert.AreEqual(1, invocationTarget.Version);
            Assert.AreEqual(2, invocationTarget.ProtocolVersionMajor);
        }
        
        [Test]
        public void ByPackageNameNoVersionJsonTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageName("counter_package_name")
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var json = JsonSerializer.Serialize(transaction);
            Assert.IsNotNull(json);
            Assert.IsFalse(json.Contains("protocol_version_major"));
        }
        
        [Test]
        public void ByPackageNameWithVersionJsonTest()
        {
            var testKey = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            var runtimeArgs = new List<NamedArg>();
            
            var transaction = new Transaction.ContractCallBuilder()
                .From(testKey.PublicKey)
                .Payment(2_500_000_000, 2)
                .ChainName("chain_name")
                .ByPackageName("counter_package_name", 1, 2)
                .EntryPoint("counter_inc")
                .RuntimeArgs(runtimeArgs)
                .Build();
            
            var json = JsonSerializer.Serialize(transaction);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"protocol_version_major\":2"));
        }
    }
}
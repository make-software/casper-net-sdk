using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.TransactionBuilder
{
    public class NativeBurnBuilder
    {
        [Test]
        public void NativeBurnBuilderTest()
        {
            var testKey = KeyPair.Create(KeyAlgo.ED25519).PublicKey;

            var transaction = new Transaction.NativeBurnBuilder()
                .From(testKey)
                .ChainName("casper-test")
                .Payment(2_500_000_000, 1)
                .Source(new URef("uref-5d7b1b23197cda53dec593caf30836a5740afa2279b356fae74bf1bdc2b2e725-000"))
                .Amount(3_000_000_000UL)
                .Build();

            var txv1 = (TransactionV1)transaction;
            Assert.IsTrue(txv1.Payload.Target is NativeTransactionV1Target);

            var nativeEntryPoint = txv1.Payload.EntryPoint as NativeTransactionV1EntryPoint;
            Assert.IsNotNull(nativeEntryPoint);
            Assert.AreEqual(NativeEntryPoint.Burn, nativeEntryPoint.Type);
            Assert.AreEqual("Burn", nativeEntryPoint.Name);

            Assert.AreEqual(2, txv1.Payload.RuntimeArgs.Count);
            Assert.AreEqual("source", txv1.Payload.RuntimeArgs[0].Name);
            Assert.AreEqual("amount", txv1.Payload.RuntimeArgs[1].Name);
            Assert.AreEqual("3000000000", txv1.Payload.RuntimeArgs[1].Value.Parsed.ToString());

            Assert.IsTrue(transaction.Invocation is Transaction.NativeTransactionInvocation);
            Assert.AreEqual(NativeEntryPoint.Burn,
                (transaction.Invocation as Transaction.NativeTransactionInvocation)!.Type);
        }
    }
}

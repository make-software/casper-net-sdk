using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class NativeBurnTransactionParse
    {
        [Test]
        public void NativeBurnTransactionParseTest()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-transaction-native-burn-v200.json");

            var result = RpcResult.Parse<GetTransactionResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);

            var transaction = result.Transaction;
            Assert.NotNull(transaction);

            var transactionV1 = (TransactionV1)transaction;
            Assert.IsNotNull(transactionV1);

            Assert.IsTrue(transactionV1.ValidateHashes(out var msg), msg);
            Assert.AreEqual("casper-test", transactionV1.Payload.ChainName);

            Assert.IsTrue(transactionV1.Payload.Target is NativeTransactionV1Target);

            var nativeEntryPoint = transactionV1.Payload.EntryPoint as NativeTransactionV1EntryPoint;
            Assert.IsNotNull(nativeEntryPoint);
            Assert.AreEqual(NativeEntryPoint.Burn, nativeEntryPoint.Type);
            Assert.AreEqual("Burn", nativeEntryPoint.Name);

            Assert.IsTrue(transaction.PricingMode is PaymentLimitedPricingMode);
            Assert.AreEqual(2500000000UL, ((PaymentLimitedPricingMode)transaction.PricingMode).PaymentAmount);
            Assert.AreEqual((byte)1, ((PaymentLimitedPricingMode)transaction.PricingMode).GasPriceTolerance);
            Assert.IsTrue(((PaymentLimitedPricingMode)transaction.PricingMode).StandardPayment);

            Assert.IsTrue(transaction.Scheduling is StandardTransactionScheduling);

            Assert.AreEqual(1, transactionV1.Payload.RuntimeArgs.Count);
            Assert.AreEqual("amount", transactionV1.Payload.RuntimeArgs[0].Name);
            Assert.AreEqual("3000000000", transactionV1.Payload.RuntimeArgs[0].Value.Parsed.ToString());

            Assert.IsTrue(transaction.Invocation is Transaction.NativeTransactionInvocation);
            Assert.AreEqual(NativeEntryPoint.Burn,
                (transaction.Invocation as Transaction.NativeTransactionInvocation)!.Type);
            Assert.AreEqual(1, transaction.Invocation.RuntimeArgs.Count);
            Assert.AreEqual("amount", transaction.Invocation.RuntimeArgs[0].Name);
            Assert.AreEqual("3000000000", transaction.Invocation.GetRuntimeArgValue("amount").Parsed.ToString());

            Assert.AreEqual(1, transactionV1.Approvals.Count);
            Assert.IsTrue(transactionV1.VerifySignatures(out var vsMsg), vsMsg);

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.AreEqual(9496612UL, result.ExecutionInfo.BlockHeight);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
            Assert.AreEqual(1, result.ExecutionInfo.ExecutionResult.CurrentGasPrice);
        }
    }
}

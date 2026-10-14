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

            Assert.AreEqual("06e0cf828f7a588efd1bbfdc31d326ef64fdcf53efd338ae38672d751139fe80", transactionV1.Hash);
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
            Assert.AreEqual("0164e74dcd32e8332a7ce3c7ad0cf77a85c64e34568e855a80ac337ad3b85b4183ad7933e784a50b9d76f5dd3e35fb4b36967a5ce73592f3818efebf0d44991b0a",
                transactionV1.Approvals[0].Signature.ToString());

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.AreEqual(9496612UL, result.ExecutionInfo.BlockHeight);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
            Assert.AreEqual(1, result.ExecutionInfo.ExecutionResult.CurrentGasPrice);
        }
    }
}

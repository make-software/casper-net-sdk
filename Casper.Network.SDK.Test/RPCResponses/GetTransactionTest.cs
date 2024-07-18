using System.IO;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetTransactionTest
    {
        [Test]
        public void GetTransactionWithDeployHashTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-transaction-deploy-v200.json");

            var result = RpcResult.Parse<GetTransactionResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            
            Assert.IsNotNull(result.Transaction);
            var deploy = (Deploy)result.Transaction;
            Assert.NotNull(deploy);
            Assert.AreEqual(deploy.Hash, result.Transaction.Hash);
            Assert.AreEqual(deploy.Header.ChainName, result.Transaction.ChainName);
            Assert.AreEqual(deploy.Header.Timestamp, result.Transaction.Timestamp);
            Assert.AreEqual(deploy.Header.Account, result.Transaction.InitiatorAddr.PublicKey);
            Assert.AreEqual("c5cf594dc6d244358fddd6461e93703653bb07a0d9fa957c4a4b5fdc5bc9968d", deploy.Header.BodyHash);
            Assert.IsNull(result.Transaction.InitiatorAddr.AccountHash);
            Assert.IsTrue(result.Transaction.Invocation is Transaction.NativeTransactionInvocation);
            Assert.AreEqual(NativeEntryPoint.Transfer, (result.Transaction.Invocation as Transaction.NativeTransactionInvocation)!.Type);
            Assert.AreEqual(TransactionCategory.Mint, result.Transaction.Category);
            Assert.IsTrue(result.Transaction.Scheduling is StandardTransactionScheduling);
            Assert.AreEqual(deploy.Approvals.Count, result.Transaction.Approvals.Count);
            Assert.AreEqual(deploy.Approvals[0].Signature, result.Transaction.Approvals[0].Signature);
            
            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.IsTrue(result.ExecutionInfo.BlockHeight > 0);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
        }
        
        [Test]
        public void GetDeployWithDeployHashTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-deploy-deploy-v200.json");

            var result = RpcResult.Parse<GetDeployResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            
            var deploy = result.Deploy;
            Assert.NotNull(deploy);
            
            var transaction = (Transaction)deploy;
            Assert.IsNotNull(transaction);
            
            Assert.AreEqual(deploy.Hash, transaction.Hash);
            Assert.AreEqual(deploy.Header.ChainName, transaction.ChainName);
            Assert.AreEqual(deploy.Header.Timestamp, transaction.Timestamp);
            Assert.AreEqual(deploy.Header.Account, transaction.InitiatorAddr.PublicKey);
            Assert.AreEqual("c5cf594dc6d244358fddd6461e93703653bb07a0d9fa957c4a4b5fdc5bc9968d", deploy.Header.BodyHash);
            Assert.IsNull(transaction.InitiatorAddr.AccountHash);
            Assert.IsTrue(transaction.Invocation is Transaction.NativeTransactionInvocation);
            Assert.AreEqual(NativeEntryPoint.Transfer, (transaction.Invocation as Transaction.NativeTransactionInvocation)!.Type);
            Assert.AreEqual(TransactionCategory.Mint, transaction.Category);
            Assert.IsTrue(transaction.Scheduling is StandardTransactionScheduling);
            Assert.AreEqual(deploy.Approvals.Count, transaction.Approvals.Count);
            Assert.AreEqual(deploy.Approvals[0].Signature, transaction.Approvals[0].Signature);

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.IsTrue(result.ExecutionInfo.BlockHeight > 0);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
        }
        
        [Test]
        public void GetTransactionWithSessionTransactionTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-transaction-session-v200.json");

            var result = RpcResult.Parse<GetTransactionResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            
            var transaction = result.Transaction;
            Assert.NotNull(transaction);
            
            var transactionV1 = (TransactionV1)transaction;
            Assert.IsNotNull(transactionV1);
            
            Assert.AreEqual(transactionV1.Hash, transaction.Hash);
            Assert.AreEqual("9b1d4002bb1af17ec9fc4193ac5ba7db6535b6dd813034b730829a94b13ebc46", transactionV1.Header.BodyHash);
            Assert.AreEqual(transactionV1.Header.ChainName, transaction.ChainName);
            Assert.AreEqual(transactionV1.Header.Timestamp, transaction.Timestamp);
            Assert.AreEqual(transactionV1.Header.InitiatorAddr.PublicKey, transaction.InitiatorAddr.PublicKey);
            Assert.IsNull(transaction.InitiatorAddr.AccountHash);
            Assert.IsTrue(transaction.Invocation is Transaction.SessionTransactionInvocation);
            Assert.AreEqual("01020304",  Hex.ToHexString((transaction.Invocation as Transaction.SessionTransactionInvocation)!.Wasm));
            Assert.AreEqual(TransactionCategory.InstallUpgrade, transaction.Category);
            Assert.IsTrue(transaction.Scheduling is StandardTransactionScheduling);
            Assert.AreEqual(transactionV1.Approvals.Count, transaction.Approvals.Count);
            Assert.AreEqual(transactionV1.Approvals[0].Signature, transaction.Approvals[0].Signature);

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.IsTrue(result.ExecutionInfo.BlockHeight > 0);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
        }
        
        [Test]
        public void GetTransactionWithStoredTransactionTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-transaction-stored-v200.json");

            var result = RpcResult.Parse<GetTransactionResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            
            var transaction = result.Transaction;
            Assert.NotNull(transaction);
            
            var transactionV1 = (TransactionV1)transaction;
            Assert.IsNotNull(transactionV1);
            
            Assert.AreEqual(transactionV1.Hash, transaction.Hash);
            Assert.AreEqual("07ed52f990a206153d2a29f6a42eb754009c4b120981dd3fd0842d4057ee7484", transactionV1.Header.BodyHash);
            Assert.AreEqual(transactionV1.Header.ChainName, transaction.ChainName);
            Assert.AreEqual(transactionV1.Header.Timestamp, transaction.Timestamp);
            Assert.AreEqual(transactionV1.Header.InitiatorAddr.AccountHash, transaction.InitiatorAddr.AccountHash);
            Assert.AreEqual("account-hash-e25f0c3b986aaa1a6c85ee356be99cd320fa1f7ceaf9928a3fbd015db11f240f", transaction.InitiatorAddr.AccountHash.ToString().ToLower());
            Assert.IsNull(transaction.InitiatorAddr.PublicKey);
            Assert.IsTrue(transaction.Invocation is Transaction.StoredTransactionInvocation);
            Assert.AreEqual("transfer", (transaction.Invocation as Transaction.StoredTransactionInvocation)!.EntryPoint);
            Assert.AreEqual(TransactionCategory.Medium, transaction.Category);
            Assert.IsTrue(transaction.Scheduling is StandardTransactionScheduling);
            Assert.AreEqual(transactionV1.Approvals.Count, transaction.Approvals.Count);
            Assert.AreEqual(transactionV1.Approvals[0].Signature, transaction.Approvals[0].Signature);

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.IsTrue(result.ExecutionInfo.BlockHeight > 0);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
        }
        
        [Test]
        public void GetTransactionWithStoredDeployTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-transaction-deploy-stored-v200.json");

            var result = RpcResult.Parse<GetTransactionResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            
            var transaction = result.Transaction;
            Assert.NotNull(transaction);
            
            var deploy = (Deploy)transaction;
            Assert.IsNotNull(deploy);
            
            Assert.AreEqual(deploy.Hash, transaction.Hash);
            Assert.AreEqual("8d18c7a727317c54fb8ac21ba2e02ec8f4d24d0f4cb4183d3df05afe86ba614c", deploy.Header.BodyHash);
            Assert.AreEqual(deploy.Header.ChainName, transaction.ChainName);
            Assert.AreEqual(deploy.Header.Timestamp, transaction.Timestamp);
            Assert.AreEqual(deploy.Header.Account, transaction.InitiatorAddr.PublicKey);
            Assert.AreEqual("0106ed45915392c02b37136618372ac8dde8e0e3b8ee6190b2ca6db539b354ede4", transaction.InitiatorAddr.PublicKey.ToString().ToLower());
            Assert.IsNull(transaction.InitiatorAddr.AccountHash);
            Assert.IsTrue(transaction.Invocation is Transaction.StoredTransactionInvocation);
            Assert.AreEqual("transfer", (transaction.Invocation as Transaction.StoredTransactionInvocation)!.EntryPoint);
            Assert.AreEqual(TransactionCategory.Large, transaction.Category);
            Assert.IsTrue(transaction.Scheduling is StandardTransactionScheduling);
             Assert.AreEqual(deploy.Approvals.Count, transaction.Approvals.Count);
            Assert.AreEqual(deploy.Approvals[0].Signature, transaction.Approvals[0].Signature);

            AssertExtensions.IsHash(result.ExecutionInfo.BlockHash);
            Assert.IsTrue(result.ExecutionInfo.BlockHeight > 0);
            Assert.IsNotNull(result.ExecutionInfo.ExecutionResult);
        }
    }
}

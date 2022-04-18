using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using System.Linq;
using System.Numerics;

namespace NetCasperTest 
{
    [Category("NCTL"), NonParallelizable]
    public class NctlTransferTest : NctlBase
    {
        private string _transferDeployHash = null;
        private TransferKey _transferKey = null;
        private string _transferBlockHash = null;
        private BigInteger _transferAmount = 2500_000_000_000;
        private KeyPair _myAccount = null; 
        
        [Test, Order(1)]
        public async Task TransferFromFaucetTest()
        {
            _myAccount = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            
            var deploy = DeployTemplates.StandardTransfer(
                _faucetKey.PublicKey,
                _myAccount.PublicKey,
                _transferAmount,
                100_000_000,
                _chainName);
            
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            _transferDeployHash = putResponse.GetDeployHash();

            Assert.AreEqual(deploy.Hash.ToLower(), _transferDeployHash.ToLower());
        }

        [Test, Order(2)]
        public async Task WaitTransferExecutionTest()
        {
            Assert.IsNotNull(_transferDeployHash, "This test must run after TransferFromFaucetTest");
            
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(_transferDeployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
            
            _transferKey = execResult.Transfers.First();
            Assert.IsNotNull(_transferKey.ToString());

            _transferBlockHash = execResult.BlockHash;
            Assert.IsNotNull(_transferBlockHash);
        }

        [Test, Order(3)]
        public async Task QueryTransferKey()
        {
            Assert.IsNotNull(_transferKey, "This test must run after WaitTransferExecutionTest");

            var getResponse = await _client.QueryGlobalState(_transferKey);
            var transfer = getResponse.Parse().StoredValue.Transfer;
            Assert.AreEqual(_transferAmount, transfer.Amount);

            var getResponse2 = await _client.QueryGlobalStateWithBlockHash(_transferKey, _transferBlockHash);
            var transfer2 = getResponse.Parse().StoredValue.Transfer;
            Assert.AreEqual(transfer.DeployHash, transfer2.DeployHash);
        }

        [Test, Order(4)]
        public async Task CatchFailedTransferTest()
        {
            Assert.IsNotNull(_transferKey, "This test must run after TransferFromFaucetTest");

            var otherAccount = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            
            var deploy = DeployTemplates.StandardTransfer(
                _myAccount.PublicKey,
                otherAccount.PublicKey,
                _transferAmount,
                _transferAmount*100, //ensure insufficient funds
                _chainName);
            
            deploy.Sign(_myAccount);

            var putResponse = await _client.PutDeploy(deploy);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deploy.Hash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsFalse(execResult.IsSuccess);
            Assert.IsFalse(string.IsNullOrWhiteSpace(execResult.ErrorMessage));
        }
        
        [Test, Order(6)]
        public async Task CatchUnknownAcctTransferTest()
        {
            var newAccount = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            
            var deploy = DeployTemplates.StandardTransfer(
                newAccount.PublicKey,
                _faucetKey.PublicKey,
                _transferAmount,
                _transferAmount*100, //ensure insufficient funds
                _chainName);
            
            deploy.Sign(newAccount);

            try
            {
                var putResponse = await _client.PutDeploy(deploy);
                Assert.Fail("Exception expected");
            }
            catch(RpcClientException ex)
            {
                Assert.IsNotNull(ex.RpcError.Message);
            }
        }

        [Test, Order(7)]
        public async Task GetBlockTransfersTest()
        {
            var response = await _client.GetBlockTransfers(_transferBlockHash);
            var result = response.Parse();
            Assert.AreEqual(1, result.Transfers.Count);
            Assert.AreEqual(_transferDeployHash, result.Transfers[0].DeployHash);
        }

        [Test, Order(8)]
        public async Task DelegateTokens()
        {
            var rpcResponse1 = _client.GetNodeStatus();
            var validatorPk = rpcResponse1.Result.Parse().OurPublicSigningKey;
            
            var wasmBytes = File.ReadAllBytes(TestContext.CurrentContext.TestDirectory +
                                              "/TestData/delegate.wasm");
            var deploy = DeployTemplates.DelegateTokens(
                wasmBytes,
                _myAccount.PublicKey,
                validatorPk,
                600_000_000_000,
                5_000_000_000,
                _chainName);
            deploy.Sign(_myAccount);
            
            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();
            
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
        }
        
        [Test, Order(9)]
        public async Task UndelegateTokens()
        {
            var rpcResponse1 = _client.GetNodeStatus();
            var validatorPk = rpcResponse1.Result.Parse().OurPublicSigningKey;
            
            var wasmBytes = File.ReadAllBytes(TestContext.CurrentContext.TestDirectory +
                                              "/TestData/undelegate.wasm");
            var deploy = DeployTemplates.DelegateTokens(
                wasmBytes,
                _myAccount.PublicKey,
                validatorPk,
                10_000_000_000,
                5_000_000_000,
                _chainName);
            deploy.Sign(_myAccount);
            
            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();
            
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
        }
    }
}
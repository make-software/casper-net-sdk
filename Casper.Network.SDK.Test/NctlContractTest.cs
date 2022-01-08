using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using System.Linq;

namespace NetCasperTest
{
    [Category("NCTL"), NonParallelizable]
    public class NctlContractTest : NctlBase
    {
        private string _wasmFile = TestContext.CurrentContext.TestDirectory +
                                   "/TestData/counter-define.wasm";

        private string _contractDeployHash = null;
        private GlobalStateKey _contractKey = null;
        private GlobalStateKey _contractPackageKey = null;
        private string _blockHash = null;

        [Test, Order(1)]
        public async Task DeployContractTest()
        {
            var wasmBytes = await File.ReadAllBytesAsync(_wasmFile);

            var deploy = DeployTemplates.ContractDeploy(
                wasmBytes,
                _faucetKey.PublicKey,
                50_000_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            _contractDeployHash = putResponse.GetDeployHash();
            Assert.AreEqual(deploy.Hash.ToLower(), _contractDeployHash.ToLower());
        }

        [Test, Order(2)]
        public async Task WaitContractDeploymentTest()
        {
            Assert.IsNotNull(_contractDeployHash, "This test must run after DeployContractTest");

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(_contractDeployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
            Assert.AreEqual(64, execResult.BlockHash.Length);
            Assert.IsNull(execResult.ErrorMessage);
        }

        [Test, Order(3)]
        public async Task QueryContractKeysTest()
        {
            var rpcResponse = await _client.GetAccountInfo(_faucetKey.PublicKey);
            var accountInfo = rpcResponse.Parse().Account;
            Assert.IsNotNull(accountInfo);

            _contractKey = accountInfo.NamedKeys.First(k => k.Name.Equals("counter")).Key;

            var rpcResponse2 = await _client.QueryGlobalState(_contractKey);
            var contractInfo = rpcResponse2.Parse().StoredValue.Contract;
            Assert.IsTrue(contractInfo.NamedKeys.First().Name.Equals("count"));

            _contractPackageKey = GlobalStateKey.FromString(contractInfo.ContractPackageHash);
            var rpcResponse3 = await _client.QueryGlobalState(_contractPackageKey);
            var contractPackageInfo = rpcResponse3.Parse().StoredValue.ContractPackage;
            Assert.IsTrue(contractPackageInfo.Versions.Count > 0);

            var contractWasmKey = GlobalStateKey.FromString(contractInfo.ContractWasmHash);
            var rpcResponse4 = await _client.QueryGlobalState(contractWasmKey);
            var contractWasmInfo = rpcResponse4.Parse().StoredValue.ContractWasm;
            Assert.IsFalse(string.IsNullOrWhiteSpace(contractWasmInfo));
        }

        [Test, Order(4)]
        public async Task CallContractByNameTest()
        {
            var deploy = DeployTemplates.ContractCall(
                "counter",
                "counter_inc",
                null,
                _faucetKey.PublicKey,
                15_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
        }

        [Test, Order(5)]
        public async Task CallContractByHashTest()
        {
            Assert.IsNotNull(_contractKey, "This test must run after QueryContractKeysTest");

            var deploy = DeployTemplates.ContractCall(
                _contractKey as HashKey,
                "counter_inc",
                null,
                _faucetKey.PublicKey,
                15_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
            _blockHash = execResult.BlockHash;
            Assert.IsNotNull(_blockHash);
        }

        // [Test, Order(6)]
        // public async Task CallVersionedContractByNameTest()
        // {
        //     var deploy = DeployTemplates.VersionedContractCall(
        //         "counter",
        //         1,
        //         "counter_inc",
        //         null,
        //         _faucetKey.PublicKey,
        //         15_000_000,
        //         _chainName);
        //     deploy.Sign(_faucetKey);
        //
        //     var putResponse = await _client.PutDeploy(deploy);
        //
        //     var deployHash = putResponse.GetDeployHash();
        //
        //     var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        //     var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);
        //
        //     var execResult = getResponse.Parse().ExecutionResults.First();
        //     Assert.IsTrue(execResult.IsSuccess);
        // }

        // [Test, Order(7)]
        // public async Task CallVersionedContractByHashTest()
        // {
        //     Assert.IsNotNull(_contractPackageKey, "This test must run after QueryContractKeysTest");
        //
        //     var deploy = DeployTemplates.VersionedContractCall(
        //         _contractPackageKey as HashKey,
        //         1,
        //         "counter_inc",
        //         null,
        //         _faucetKey.PublicKey,
        //         15_000_000,
        //         _chainName);
        //     deploy.Sign(_faucetKey);
        //
        //     var putResponse = await _client.PutDeploy(deploy);
        //
        //     var deployHash = putResponse.GetDeployHash();
        //
        //     var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        //     var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);
        //
        //     var execResult = getResponse.Parse().ExecutionResults.First();
        //     Assert.IsTrue(execResult.IsSuccess);
        // }
        //
        [Test, Order(8)]
        public async Task QueryContractState()
        {
            Assert.IsNotNull(_blockHash, "This test must run after CallContractByHashTest");
        
            var accountKey = new AccountHashKey(_faucetKey.PublicKey);
            var rpcResponse = await _client.QueryGlobalStateWithBlockHash(accountKey, _blockHash,
                "counter/count");
        
            var result = rpcResponse.Parse();
            Assert.IsTrue((int) result.StoredValue.CLValue > 0);
        }
        
        [Test, Order(9)]
        public async Task GetItemTest()
        {
            var rpcResponse = await _client.QueryState(_faucetKey.PublicKey.GetAccountHash(),
                new List<string> {"counter", "count"});
            var count = rpcResponse.Parse().StoredValue.CLValue;
            Assert.IsNotNull(count);
            Assert.IsTrue((int)count > 0);
        }
    }
}
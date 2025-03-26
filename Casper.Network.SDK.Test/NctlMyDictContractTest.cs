using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;

namespace NetCasperTest
{
    [Category("NCTL"), NonParallelizable]
    public class NctlMyDictContractTest : NctlBase
    {
        private string _wasmFile = TestContext.CurrentContext.TestDirectory +
                                   "/TestData/mydict-contract.wasm";

        private string _contractDeployHash = null;
        private GlobalStateKey _contractKey = null;
        private GlobalStateKey _contractPackageKey = null;
        private string _dictionaryKey = null;
        
        [Test, Order(1)]
        public async Task DeployContractTest()
        {
            var wasmBytes = await FileExtensions.ReadAllBytesAsync(_wasmFile);

            var deploy = DeployTemplates.ContractDeploy(
                wasmBytes,
                _faucetKey.PublicKey,
                200_000_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            _contractDeployHash = putResponse.Parse().DeployHash;
            Assert.AreEqual(deploy.Hash.ToLower(), _contractDeployHash.ToLower());
        }

        [Test, Order(2)]
        public async Task WaitContractDeploymentTest()
        {
            Assert.IsNotNull(_contractDeployHash, "This test must run after DeployContractTest");

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(_contractDeployHash, tokenSource.Token);

            var execInfo = getResponse.Parse().ExecutionInfo;
            var execResult = execInfo.ExecutionResult;
            Assert.IsTrue(execResult.IsSuccess);
            AssertExtensions.IsHash(execInfo.BlockHash);
            Assert.IsNull(execResult.ErrorMessage);
        }

        [Test, Order(3)]
        public async Task CallVersionedContractByNameTest()
        {
            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "myString1"),
                new NamedArg("value", "first-entry")
            };

            var deploy = DeployTemplates.VersionedContractCall(
                "mydict_contract_package",
                1,
                "store_mydict",
                namedArgs,
                _faucetKey.PublicKey,
                1_000_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execInfo = getResponse.Parse().ExecutionInfo;
            var execResult = execInfo.ExecutionResult;
            Assert.IsTrue(execResult.IsSuccess);
        }

        [Test, Order(4)]
        public async Task CallVersionedContractByHashTest()
        {
            var rpcResponse = await _client.GetAccountInfo(_faucetKey.PublicKey);
            var accountInfo = rpcResponse.Parse().Account;
            Assert.IsNotNull(accountInfo);

            _contractKey = accountInfo.NamedKeys
                .First(k => k.Name.Equals("mydict_contract")).Key;
            Assert.IsNotNull(_contractKey);
            
            _contractPackageKey = accountInfo.NamedKeys
                .First(k => k.Name.Equals("mydict_contract_package")).Key;
            Assert.IsNotNull(_contractPackageKey);

            var namedArgs = new List<NamedArg>()
            {
                new NamedArg("name", "myString2"),
                new NamedArg("value", "second-entry")
            };

            var deploy = DeployTemplates.VersionedContractCall(
                _contractPackageKey as HashKey,
                1,
                "store_mydict",
                namedArgs,
                _faucetKey.PublicKey,
                1_000_000_000,
                _chainName);
            deploy.Sign(_faucetKey);

            var putResponse = await _client.PutDeploy(deploy);

            var deployHash = putResponse.GetDeployHash();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deployHash, tokenSource.Token);

            var execInfo = getResponse.Parse().ExecutionInfo;
            var execResult = execInfo.ExecutionResult;
            Assert.IsTrue(execResult.IsSuccess);
        }

        [Test, Order(5)]
        public async Task GetDictItemByContractTest()
        {
            Assert.IsNotNull(_contractKey, "This test must run after CallVersionedContractByHashTest");

            var rpcResponse = await _client.GetDictionaryItemByContract(_contractKey.ToString(),
                "mydict", "myString1");

            var result = rpcResponse.Parse();
            Assert.AreEqual("first-entry", (string) result.StoredValue.CLValue);
        }

        [Test, Order(6)]
        public async Task GetDictItemByAccountTest()
        {
            var rpcResponse = await _client.GetDictionaryItemByAccount(_faucetKey.PublicKey.GetAccountHash(),
                "mydict", "myString2");

            var result = rpcResponse.Parse();
            Assert.AreEqual("second-entry", (string) result.StoredValue.CLValue);
        }
        
        [Test, Order(7)]
        public async Task GetDictItemByURef()
        {
            var aiResponse = await _client.GetAccountInfo(_faucetKey.PublicKey);
            var myDictKey = aiResponse.Parse().Account.NamedKeys.First(k => k.Name.Equals("mydict")).Key;
            
            var rpcResponse = await _client.GetDictionaryItemByURef(myDictKey.ToString(),
                "myString2");

            var result = rpcResponse.Parse();
            Assert.AreEqual("second-entry", (string) result.StoredValue.CLValue);

            _dictionaryKey = result.DictionaryKey;
        }
        
        [Test, Order(8)]
        public async Task GetDictItemByKey()
        {
            var aiResponse = await _client.GetAccountInfo(_faucetKey.PublicKey);
            var myDictKey = aiResponse.Parse().Account.NamedKeys.First(k => k.Name.Equals("mydict")).Key;
            
            var rpcResponse = await _client.GetDictionaryItem(_dictionaryKey);

            var result = rpcResponse.Parse();
            Assert.AreEqual("second-entry", (string) result.StoredValue.CLValue);
        }
    }
}

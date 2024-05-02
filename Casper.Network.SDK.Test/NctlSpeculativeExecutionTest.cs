using System;
using System.IO;
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
    public class NctlSpeculativeExecutionTest
    {
        protected string _nodeAddress;
        protected string _chainName = "casper-net-1";
        protected NetCasperClient _client;
        protected KeyPair _faucetKey;
        
        private string _wasmFile = TestContext.CurrentContext.TestDirectory +
                                   "/TestData/counter-define.wasm";

        [SetUp]
        public void Setup()
        {
            _nodeAddress = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_ADDRESS");
            Assert.IsNotNull(_nodeAddress,
                "Please, set environment variable CASPERNETSDK_NODE_ADDRESS with a valid node url (with port).");
            _nodeAddress = _nodeAddress.Replace("11101", "25101");

            _client = new NetCasperClient(_nodeAddress);

            var fkFilename = TestContext.CurrentContext.TestDirectory +
                             "/TestData/faucetact.pem";
            _faucetKey = KeyPair.FromPem(fkFilename);
            Assert.IsNotNull(_faucetKey, $"Cannot read faucet key from '{fkFilename}");
        }
        
        [Test, Order(1)]
        public async Task SpeculativeExecutionTest()
        {
            var wasmBytes = await FileExtensions.ReadAllBytesAsync(_wasmFile);

            var deploy = DeployTemplates.ContractDeploy(
                wasmBytes,
                _faucetKey.PublicKey,
                50_000_000_000,
                _chainName,
                1, //gasPrice=1
                3_600_000*2); //ttl='2h'
            deploy.Sign(_faucetKey);

            var rpcResponse = await _client.SpeceulativeExecution(deploy);

            var result = rpcResponse.Parse();
            Assert.IsNotNull(result.BlockHash);
            Assert.IsNotNull(result.ExecutionResult);
            Assert.IsTrue(result.ExecutionResult.Effect.Transforms.Count > 0);
        }
    }
    
    
}

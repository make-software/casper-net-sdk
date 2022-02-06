using System;
using Casper.Network.SDK;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class NctlBase
    {
        protected string _nodeAddress;
        protected string _chainName = "casper-net-1";
        protected NetCasperClient _client;
        protected KeyPair _faucetKey;
        
        [SetUp]
        public void Setup()
        {
            _nodeAddress = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_ADDRESS");
            Assert.IsNotNull(_nodeAddress,
                "Please, set environment variable CASPERNETSDK_NODE_ADDRESS with a valid node url (with port).");

            _client = new NetCasperClient(_nodeAddress);

            var fkFilename = TestContext.CurrentContext.TestDirectory +
                             "/TestData/faucetact.pem";
            _faucetKey = KeyPair.FromPem(fkFilename);
            Assert.IsNotNull(_faucetKey, $"Cannot read faucet key from '{fkFilename}");
        }
    }
}
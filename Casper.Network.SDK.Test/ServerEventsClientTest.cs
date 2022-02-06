using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    [Category("NCTL-SSE")]
    public class ServerEventsClientTest
    {
        private string _nodeAddress;
        private string _chainName = "casper-net-1";
        private NetCasperClient _client;
        private KeyPair _faucetKey;
        private string nodeIpSSE;
        private int nodePortSSE;
        
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

            nodeIpSSE = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_SSE_IP");
            Assert.IsNotNull(nodeIpSSE,
                "Please, set environment variable CASPERNETSDK_NODE_SSE_IP with a valid node ip.");

            var port = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_SSE_PORT");
            Assert.IsNotNull(port,
                "Please, set environment variable CASPERNETSDK_NODE_SSE_PORT with a valid node ip.");
            Assert.IsTrue(int.TryParse(port, out nodePortSSE));
        }
        
        [Test]
        public void ListenBlocksAdded()
        {
            int nBlocks = 0;
            
            var sse = new ServerEventsClient(nodeIpSSE, nodePortSSE);

            sse.AddEventCallback(EventType.BlockAdded, "catch-blocks-cb",
                (SSEvent evt) =>
                {
                    try
                    {
                        if (evt.EventType == EventType.BlockAdded)
                        {
                            var block = evt.Parse<BlockAdded>();
                            Assert.IsNotNull(block.BlockHash);
                            nBlocks++;
                        }
                        else
                        {
                            Assert.Fail($"No event different from BlockAdded expected. Received: '{evt.EventType}'");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                },
                startFrom: 0);

            sse.StartListening();

            int retries = 0;
            while(nBlocks < 3 && retries++ < 5)
                Thread.Sleep(5000);

            sse.StopListening().Wait();

            Assert.IsTrue(nBlocks >= 3);
        }

        private async Task MakeTransfer()
        {
            KeyPair myAccount = KeyPair.CreateNew(KeyAlgo.SECP256K1);
            
            var deploy = DeployTemplates.StandardTransfer(
                _faucetKey.PublicKey,
                myAccount.PublicKey,
                2500_000_000_000,
                100_000_000,
                _chainName);
            
            deploy.Sign(_faucetKey);

            await _client.PutDeploy(deploy);
            
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var getResponse = await _client.GetDeploy(deploy.Hash, tokenSource.Token);

            var execResult = getResponse.Parse().ExecutionResults.First();
            Assert.IsTrue(execResult.IsSuccess);
        }

        [Test]
        public async Task SSEListen()
        {
            int nBlocks = 0;
            int nSignatures = 0;
            int nApiVersion = 0;
            int nDeployAccepted = 0;
            int nDeployProcessed = 0;
            
            var sse = new ServerEventsClient(nodeIpSSE, nodePortSSE);

            sse.AddEventCallback(EventType.All, "catch-all-cb",
                (SSEvent evt) =>
                {
                    try
                    {
                        if (evt.EventType == EventType.FinalitySignature)
                        {
                            var sig = evt.Parse<FinalitySignature>();
                            Assert.IsNotNull(sig.BlockHash);
                            nSignatures++;
                        }
                        else if (evt.EventType == EventType.BlockAdded)
                        {
                            var block = evt.Parse<BlockAdded>();
                            Assert.IsNotNull(block.BlockHash);
                            nBlocks++;
                        }
                        else if (evt.EventType == EventType.DeployAccepted)
                        {
                            var deploy = evt.Parse<DeployAccepted>();
                            Assert.IsNotNull(deploy.Hash);
                            nDeployAccepted++;
                        }
                        else if (evt.EventType == EventType.DeployProcessed)
                        {
                            var deploy = evt.Parse<DeployProcessed>();
                            Assert.IsNotNull(deploy.BlockHash);
                            nDeployProcessed++;
                        }
                        else if (evt.EventType == EventType.ApiVersion)
                        {
                            nApiVersion++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                },
                startFrom: 0);

            sse.StartListening();

            // send a deploy to generate DeployAccepted and DeployProcessed events
            //
            await MakeTransfer();

            int retries = 0;
            while(nDeployProcessed == 0 && retries++ < 5)
                Thread.Sleep(5000);

            sse.StopListening().Wait();

            Assert.IsTrue(nApiVersion  > 0);
            Assert.IsTrue(nBlocks > 0);
            Assert.IsTrue(nSignatures > 0);
            Assert.IsTrue(nDeployAccepted > 0);
            Assert.IsTrue(nDeployProcessed > 0);
        }
    }
}
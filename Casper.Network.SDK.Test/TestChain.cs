using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using NUnit.Framework;

namespace NetCasperTest
{
    public class TestChain
    {
        private string _nodeAddress;
        private string _accountPublicKey;

        [SetUp]
        public void Setup()
        {
            _nodeAddress = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_ADDRESS");
            Assert.IsNotNull(_nodeAddress,
                "Please, set environment variable CASPERNETSDK_NODE_ADDRESS with a valid node url (with port).");
            _accountPublicKey = Environment.GetEnvironmentVariable("CASPERNETSDK_ACCPUBLICKEY");
            Assert.IsNotNull(_accountPublicKey,
                "Please, set environment variable CASPERNETSDK_ACCPUBLICKEY with a valid account public key.");
        }

        [Test]
        public async Task TestChainSimpleQueries_v_0_2_0()
        {
            var casperSdk = new NetCasperClient(_nodeAddress);

            var auctionInfoResp = await casperSdk.GetAuctionInfo();
            Assert.IsNotNull(auctionInfoResp.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(auctionInfoResp.Result.GetProperty("auction_state").GetProperty("state_root_hash")
                .GetString());

            var nodeStatusResp = await casperSdk.GetNodeStatus();
            Assert.IsNotNull(nodeStatusResp.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(nodeStatusResp.Result.GetProperty("chainspec_name").GetString());
            Assert.IsNotEmpty(nodeStatusResp.Result.GetProperty("last_added_block_info").GetRawText());
            Assert.IsNotNull(nodeStatusResp.Result.GetProperty("last_added_block_info").GetProperty("era_id"));
            Assert.IsNotNull(nodeStatusResp.Result.GetProperty("peers")[0].GetProperty("node_id").GetString());

            var nodeStatus = nodeStatusResp.Parse();

            var nodePeersResp = await casperSdk.GetNodePeers();
            Assert.IsNotNull(nodePeersResp.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(nodePeersResp.Result.GetProperty("peers")[0].GetProperty("node_id").GetString());
        }

        [Test]
        public async Task TestAgainstChain()
        {
            var casperSdk = new NetCasperClient(_nodeAddress);

            try
            {
                var accInfoResult = await casperSdk.GetAccountInfo(_accountPublicKey);

                var mainPurse = accInfoResult.Result.GetProperty("account").GetProperty("main_purse").GetString();

                var rpcResponse = await casperSdk.GetAccountBalance(mainPurse);

                var accountBalance = rpcResponse.Result.GetProperty("balance_value").GetString();

                Assert.IsNotEmpty(accountBalance);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }

        }

        [Test]
        public async Task NodeMetricsTest()
        {
            var nodeAddress = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_METRICS_ADDRESS");
            Assert.IsNotNull(nodeAddress,
                "Please, set environment variable CASPERNETSDK_NODE_METRICS_ADDRESS with a valid node url endpoint for metrics.");

            var nodeIp = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_METRICS_IP");
            Assert.IsNotNull(nodeIp,
                "Please, set environment variable CASPERNETSDK_NODE_METRICS_IP with a valid node ip.");

            var nodePortStr = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_METRICS_PORT");
            Assert.IsNotNull(nodePortStr,
                "Please, set environment variable CASPERNETSDK_NODE_METRICS_PORT with a valid node ip.");
            Assert.IsTrue(int.TryParse(nodePortStr, out var nodePort));

            try
            {
                var metrics = await NetCasperClient.GetNodeMetrics(nodeIp, nodePort);
                Assert.IsFalse(string.IsNullOrEmpty(metrics));
                Assert.IsTrue(metrics.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 0);

                var metricsWithUri = await NetCasperClient.GetNodeMetrics(nodeAddress);
                Assert.IsFalse(string.IsNullOrEmpty(metricsWithUri));
                Assert.IsTrue(metricsWithUri.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
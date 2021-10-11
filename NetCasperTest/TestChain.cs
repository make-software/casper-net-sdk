using System;
using System.Threading.Tasks;
using NetCasperSDK;
using NetCasperSDK.JsonRpc;
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
            _nodeAddress = Environment.GetEnvironmentVariable("NETCASPERSDK_NODE_ADDRESS");
            Assert.IsNotNull(_nodeAddress,
                "Please, set environment variable NETCASPERSDK_NODE_ADDRESS with a valid node url (with port).");
            _accountPublicKey = Environment.GetEnvironmentVariable("NETCASPERSDK_ACCPUBLICKEY");
            Assert.IsNotNull(_accountPublicKey,
                "Please, set environment variable NETCASPERSDK_ACCPUBLICKEY with a valid account public key.");
        }

        [Test]
        public async Task TestChainSimpleQueries_v_0_2_0()
        {
            var casperSdk = new NetCasperClient(_nodeAddress);

            var rpcResponse = await casperSdk.GetAuctionInfo();
            Assert.IsNotNull(rpcResponse.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(rpcResponse.Result.GetProperty("auction_state").GetProperty("state_root_hash")
                .GetString());

            rpcResponse = await casperSdk.GetNodeStatus();
            Assert.IsNotNull(rpcResponse.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(rpcResponse.Result.GetProperty("chainspec_name").GetString());
            Assert.IsNotEmpty(rpcResponse.Result.GetProperty("last_added_block_info").GetRawText());
            Assert.IsNotNull(rpcResponse.Result.GetProperty("last_added_block_info").GetProperty("era_id"));
            Assert.IsNotNull(rpcResponse.Result.GetProperty("peers")[0].GetProperty("node_id").GetString());

            rpcResponse = await casperSdk.GetNodePeers();
            Assert.IsNotNull(rpcResponse.Result.GetProperty("api_version").GetString());
            Assert.IsNotNull(rpcResponse.Result.GetProperty("peers")[0].GetProperty("node_id").GetString());
        }

        [Test]
        public async Task TestAgainstChain()
        {
            var casperSdk = new NetCasperClient(_nodeAddress);

            try
            {
                var rpcResponse = await casperSdk.GetAccountInfo(_accountPublicKey);

                var mainPurse = rpcResponse.Result.GetProperty("stored_value")
                    .GetProperty("Account").GetProperty("main_purse").GetString();

                rpcResponse = await casperSdk.GetAccountBalance(mainPurse);

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
            var nodeAddress = Environment.GetEnvironmentVariable("NETCASPERSDK_NODE_METRICS_ADDRESS");
            Assert.IsNotNull(nodeAddress,
                "Please, set environment variable NETCASPERSDK_NODE_METRICS_ADDRESS with a valid node url endpoint for metrics.");
            
            var nodeIp = Environment.GetEnvironmentVariable("NETCASPERSDK_NODE_METRICS_IP");
            Assert.IsNotNull(nodeIp,
                "Please, set environment variable NETCASPERSDK_NODE_METRICS_IP with a valid node ip.");
            
            var nodePortStr = Environment.GetEnvironmentVariable("NETCASPERSDK_NODE_METRICS_PORT");
            Assert.IsNotNull(nodePortStr,
                "Please, set environment variable NETCASPERSDK_NODE_METRICS_PORT with a valid node ip.");
            Assert.IsTrue(int.TryParse(nodePortStr, out var nodePort));
            
            try
            {
                var metrics = await NetCasperClient.GetNodeMetrics(nodeIp, nodePort);
                Assert.IsFalse(string.IsNullOrEmpty(metrics));
                Assert.IsTrue(metrics.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries).Length > 0);

                var metricsWithUri = await NetCasperClient.GetNodeMetrics(nodeAddress);
                Assert.IsFalse(string.IsNullOrEmpty(metricsWithUri));
                Assert.IsTrue(metricsWithUri.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries).Length > 0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
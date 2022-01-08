using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using NUnit.Framework;

namespace NetCasperTest
{
    [Category("NCTL")]
    public class NctlNodeMetricsTest
    {
        [Test]
        public async Task NodeMetricsTest()
        {
            var nodeIp = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_METRICS_IP");
            Assert.IsNotNull(nodeIp,
                "Please, set environment variable CASPERNETSDK_NODE_METRICS_IP with a valid node ip.");

            var nodePortStr = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_METRICS_PORT");
            Assert.IsNotNull(nodePortStr,
                "Please, set environment variable CASPERNETSDK_NODE_METRICS_PORT with a valid node ip.");
            Assert.IsTrue(int.TryParse(nodePortStr, out var nodePort));

            var nodeAddress = $"http://{nodeIp}:{nodePortStr}/metrics";
                
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

        [Test]
        public async Task NodeMetricsWrongPortTest()
        {
            //wrong node address
            var nodeAddress = $"http://127.0.0.1:11101/metrics";
                
            try
            {
                await NetCasperClient.GetNodeMetrics(nodeAddress);
                Assert.Fail("Exception expected for a wrong REST API endpoint");
            }
            catch (Exception)
            {
                // exception catched
            }
        }
    }
}
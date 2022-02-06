using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using NUnit.Framework;

namespace NetCasperTest
{
    public class NctlRpcLoggingHandler
    {
        [Test, Category("NCTL")]
        public async Task Log2File()
        {
            var nodeAddress = Environment.GetEnvironmentVariable("CASPERNETSDK_NODE_ADDRESS");
            Assert.IsNotNull(nodeAddress,
                "Please, set environment variable CASPERNETSDK_NODE_ADDRESS with a valid node url (with port).");

            var tmpfile = System.IO.Path.GetTempFileName();
            var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
            {
                LoggerStream = new StreamWriter(File.OpenWrite(tmpfile))
            };
            
            var client = new NetCasperClient(nodeAddress, loggingHandler);
            await client.GetNodeStatus();

            client.Dispose();
            var data = File.ReadAllText(tmpfile);
            Assert.IsTrue(data.Contains("Request"));   
            Assert.IsTrue(data.Contains("Response"));
            Assert.IsTrue(data.Contains("our_public_signing_key"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace Casper.NET.SDK.Examples
{
    class Chainspec
    {
        private Dictionary<string, string> data;

        public Chainspec(string configContent)
        {
            data = new Dictionary<string, string>();
            ParseContent(configContent);
        }

        private void ParseContent(string content)
        {
            string currentSection = null;
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Section header
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                }
                else if (currentSection != null && trimmedLine.Contains("="))
                {
                    var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim().Trim('"');
                        data[currentSection + "." + key] = value;
                    }
                }
            }
        }

        public string GetValue(string key)
        {
            return data.GetValueOrDefault(key);
        }
    }
    
    public class GetChainspec
    {
        public static async Task Main(string[] args)
        {
            string nodeAddress = "http://127.0.0.1:11101/rpc";

            try
            {
                // create an instance of the NetCasperClient that logs requests/outputs in stdout
                //
                var loggingHandler = new RpcLoggingHandler(new HttpClientHandler())
                {
                    LoggerStream = new StreamWriter(Console.OpenStandardOutput())
                };
                var casperSdk = new NetCasperClient(nodeAddress, loggingHandler);
                
                // get node status and print API version and PK as example
                //
                var rpcResponse = await casperSdk.GetChainspec();
                var result = rpcResponse.Parse();

                var chainspec = new Chainspec(result.ChainspecBytes.ChainspecAsString);

                Console.WriteLine("Consensus protocol : " + chainspec.GetValue("core.consensus_protocol"));
                Console.WriteLine("Pricing mode       : " + chainspec.GetValue("core.pricing_handling.type"));
                Console.WriteLine("Max gas price      : " + chainspec.GetValue("vacancy.max_gas_price"));
                Console.WriteLine("Wasm tx lanes      : " + chainspec.GetValue("transactions.v1.wasm_lanes"));
            }
            catch (RpcClientException e)
            {
                Console.WriteLine("ERROR:\n" + e.RpcError.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
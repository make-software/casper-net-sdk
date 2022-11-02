using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.NET.SDK.Examples
{
    public class ParseDictionaries
    {
        public static async Task Main(string[] args)
        {
            // string nodeAddress = "http://3.138.177.248:7777/rpc";
            string nodeAddress = "http://3.12.207.193:7777/rpc"; //mainnet
            // string nodeAddress = "http://testnet-node.make.services:7777/rpc"; //testnet
            // string nodeAddress = "http://127.0.0.1:11101/rpc";

            try
            {
                // create an instance of the NetCasperClient that logs requests/outputs in stdout
                //
                var clientHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                };
                var loggingHandler = new RpcLoggingHandler(clientHandler)
                {
                    LoggerStream = new StreamWriter(Console.OpenStandardOutput())
                };
                var client = new HttpClient(loggingHandler);
                
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip");

                var casperSdk = new NetCasperClient(nodeAddress, client);
                
                var deploys = new List<string>()
                {
                    "acae3f5d8287d6808c48c199e2f0fb64dcf5e547b169adaeabd2ff9a133a633e", // Casper Army - mint deploy
                    "250229cfd969bd99ff862087456676c304ef8b639ac6d21020668733f057eed3", // GoblinTitties - wasm deploy
                    "3299c446ca0c1e8b3cd7882b9eacb29520f3de29d1a4c22ae13612aaae7421d9" // Verified Impact - mint deploy
                };
                
                var rpcResponse = await casperSdk.GetDeploy(deploys[2]);
                
                var result = rpcResponse.Parse();
                var dictionaries = result.ExecutionResults.First().Effect.Transforms
                    .Where(r => r.Type == TransformType.WriteCLValue && r.Key is DictionaryKey);
                
                foreach (var dictTransform in dictionaries)
                {
                    var bytes = (dictTransform.Value as CLValue)?.Bytes;
                    var entry = DictionaryEntry.FromBytes(bytes);

                    PrintDictionaryEntry(dictTransform.Key.ToString(), entry);
                }

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

        private static void PrintDictionaryEntry(string key, DictionaryEntry entry)
        {

            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("Dictionary Seed URef:       " + entry.DictionarySeed);
            Console.WriteLine("Dictionary Entry Key:       " + key);
            Console.WriteLine("Dictionary Entry Item Key:  " + entry.ItemKey);

            Console.WriteLine("Dictionary Entry item type: " + entry.Value.TypeInfo);

            if (entry.Value.TypeInfo is CLMapTypeInfo ||
            entry.Value.TypeInfo is CLOptionTypeInfo option && option.OptionType is CLMapTypeInfo)
            {
                Console.WriteLine("DictionaryEntry value:");
                var parsed = entry.Value.Some<Dictionary<string,string>>(out var dict);
                
                if(parsed)
                    foreach (var kvp in dict)
                    {
                        Console.WriteLine($"    {kvp.Key} - {kvp.Value}");
                    }
            }
            else
            {
                Console.WriteLine("DictionaryEntry value:      " + entry.Value);
            }
        }
    }
}

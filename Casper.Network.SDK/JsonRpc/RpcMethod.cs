using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc
{
    /// <summary>
    /// Wrapper class for an RPC call method that can be serialized to JSON.
    /// </summary>
    public abstract class RpcMethod
    {
        /// <summary>
        /// Version of the RPC protocol in use
        /// </summary>
        [JsonPropertyName("jsonrpc")] 
        public string JsonRpc { get; } = "2.0";

        /// <summary>
        /// Id of the RPC request that can be correlated with the equivalent Id in the RPC response
        /// </summary>
        [JsonPropertyName("id")] 
        public uint Id { get; set; } = 1;

        /// <summary>
        /// Method name.
        /// </summary>
        [JsonPropertyName("method")] 
        public string Method { get; private set; }

        /// <summary>
        /// List of parameters that are included in the RPC Request
        /// </summary>
        [JsonPropertyName("params")] 
        public Dictionary<string, object> Parameters { get; protected set; }

        public static JsonSerializerOptions SerializerOptions
        {
            get
            {
                return new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new PublicKey.PublicKeyConverter(),
                        new ExecutableDeployItemConverter()
                    }
                };
            }
        }
        
        public RpcMethod(string method)
        {
            this.Method = method;
        }

        public RpcMethod(string method, string blockHash)
        {
            this.Method = method;

            this.Parameters = blockHash switch
            {
                null => new Dictionary<string, object>(),
                _ => new Dictionary<string, object>
                {
                    {
                        "block_identifier", new Dictionary<string, string>
                        {
                            {"Hash", blockHash}
                        }
                    }
                }
            };
        }

        public RpcMethod(string method, int blockHeight)
        {
            this.Method = method;

            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", blockHeight}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier}
            };
        }

        /// <summary>
        /// Converts an RpcMethod derived object a JSON string that can be sent to the network  
        /// </summary>
        public string Serialize()
        {
            return JsonSerializer.Serialize(this, typeof(RpcMethod), SerializerOptions);
        }
    }
}

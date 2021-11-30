using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc
{
    public abstract class RpcMethod
    {
        [JsonPropertyName("jsonrpc")] 
        public string JsonRpc { get; } = "2.0";

        [JsonPropertyName("id")] 
        public uint Id { get; set; } = 1;

        [JsonPropertyName("method")] 
        public string Method { get; private set; }

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

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, typeof(RpcMethod), SerializerOptions);
        }
    }
}
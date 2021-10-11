using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    public class DeployHeader
    {
        [JsonPropertyName("account")]
        public PublicKey Account { get; set; }
        
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTime2EpochConverter))]
        public ulong Timestamp { get; set; }
        
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }
        
        [JsonPropertyName("dependencies")]
        public List<byte[]> Dependencies { get; set; }
        
        [JsonPropertyName("gas_price")]
        public ulong GasPrice { get; set; }
        
        [JsonPropertyName("body_hash")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] BodyHash { get; set; }
        
        [JsonPropertyName("chain_name")]
        public string ChainName { get; set; }

        public DeployHeader()
        {
            Dependencies = new List<byte[]>();
        }
    }
}
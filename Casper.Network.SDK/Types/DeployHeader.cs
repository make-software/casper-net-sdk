using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Header information of a Deploy.
    /// </summary>
    public class DeployHeader
    {
        /// <summary>
        /// Public Key from the Account owning the Deploy.
        /// </summary>
        [JsonPropertyName("account")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Account { get; set; }
        
        /// <summary>
        /// Timestamp formatted as per RFC 3339 
        /// </summary>
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTime2EpochConverter))]
        public ulong Timestamp { get; set; }
        
        /// <summary>
        /// Duration of the Deploy in milliseconds (from timestamp).
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }
        
        /// <summary>
        /// List of Deploy hashes.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public List<string> Dependencies { get; set; }
        
        /// <summary>
        /// Gas price
        /// </summary>
        [JsonPropertyName("gas_price")]
        public ulong GasPrice { get; set; }
        
        /// <summary>
        /// Hash of the body part of this Deploy.
        /// </summary>
        [JsonPropertyName("body_hash")]
        [JsonConverter(typeof(CEP57Checksum.HashWithChecksumConverter))]
        public string BodyHash { get; set; }
        
        /// <summary>
        /// Name of the chain where the deploy is executed.
        /// </summary>
        [JsonPropertyName("chain_name")]
        public string ChainName { get; set; }

        public DeployHeader()
        {
            Dependencies = new List<string>();
        }
    }
}
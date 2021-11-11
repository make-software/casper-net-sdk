using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetCasperSDK.Types
{
    public class DisabledVersion
    {
        [JsonPropertyName("contract_version")]
        public uint Version { get; init; }
        
        [JsonPropertyName("protocol_version_major")]
        public uint ProtocolVersionMajor { get; init; }
    }
    
    public class ContractVersion
    {
        [JsonPropertyName("contract_hash")]
        public string Hash { get; init; }
        
        [JsonPropertyName("contract_version")]
        public uint Version { get; init; }
        
        [JsonPropertyName("protocol_version_major")]
        public uint ProtocolVersionMajor { get; init; }
    }
    
    /// <summary>
    /// Contract definition, metadata, and security container.
    /// </summary>
    public class ContractPackage
    {
        [JsonPropertyName("access_key")]
        public string AccessKey { get; init; }
        
        [JsonPropertyName("disabled_versions")]
        public List<DisabledVersion> DisabledVersions { get; init; }
        
        [JsonPropertyName("groups")]
        public List<string> Groups { get; init; }
        
        [JsonPropertyName("versions")]
        public List<ContractVersion> Versions { get; init; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
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
    /// Groups associate a set of URefs with a label.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Group label 
        /// </summary>
        [JsonPropertyName("group")]
        public string Label { get; init; }
        
        /// <summary>
        /// List of URefs associated with the group label.
        /// </summary>
        [JsonPropertyName("keys")]
        [JsonConverter(typeof(GenericListConverter<URef, URef.URefConverter>))]
        public List<URef> Keys { get; init; }
    }
    
    /// <summary>
    /// Contract definition, metadata, and security container.
    /// </summary>
    public class ContractPackage
    {
        [JsonPropertyName("access_key")]
        [JsonConverter(typeof(URef.URefConverter))]
        public URef AccessKey { get; init; }
        
        [JsonPropertyName("disabled_versions")]
        public List<DisabledVersion> DisabledVersions { get; init; }
        
        /// <summary>
        /// Groups associate a set of URefs with a label. Entry points on a contract can be given
        /// a list of labels they accept and the runtime will check that a URef from at least one
        /// of the allowed groups is present in the caller’s context before execution.
        /// </summary>
        [JsonPropertyName("groups")]
        public List<Group> Groups { get; init; }
        
        [JsonPropertyName("versions")]
        public List<ContractVersion> Versions { get; init; }
    }
}
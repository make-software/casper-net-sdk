using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Major element of `ProtocolVersion` combined with `EntityVersion`.
    /// </summary>
    public class EntityVersion
    {
        /// <summary>
        /// Automatically incremented value for a contract version within a major `ProtocolVersion`.
        /// </summary>
        [JsonPropertyName("entity_version")]
        public uint Version { get; init; }

        /// <summary>
        /// Major element of `ProtocolVersion` a `ContractVersion` is compatible with.
        /// </summary>
        [JsonPropertyName("protocol_version_major")]
        public uint ProtocolVersionMajor { get; init; }
    }

    public enum PackageStatus
    {
        /// <summary>
        /// The package is locked and cannot be versioned.
        /// </summary>
        Locked,

        /// <summary>
        /// The package is unlocked and can be versioned.
        /// </summary>
        Unlocked,
    }

    public class EntityVersionAndHash
    {
        /// <summary>
        /// Major element of `ProtocolVersion` combined with `EntityVersion`.
        /// </summary>
        [JsonPropertyName("entity_version_key")]
        public EntityVersion EntityVersion { get; init; }

        /// <summary>
        /// The hex-encoded address of the addressable entity.
        /// </summary>
        [JsonPropertyName("addressable_entity_hash")]
        public string AddressableEntityHash { get; init; }
    }

    public class NamedUserGroup
    {
        /// <summary>
        /// The hex-encoded address of the addressable entity.
        /// </summary>
        [JsonPropertyName("group_name")]
        public string Name { get; init; }

        /// <summary>
        /// List of URefs associated with the group.
        /// </summary>
        [JsonPropertyName("group_users")]
        [JsonConverter(typeof(GenericListConverter<URef, GlobalStateKey.GlobalStateKeyConverter>))]
        public List<URef> Users { get; init; }
    }

    /// <summary>
    /// Entity definition, metadata, and security container.
    /// </summary>
    public class Package
    {
        /// <summary>
        /// All versions (enabled and disabled).
        /// </summary>
        [JsonPropertyName("versions")]
        public List<EntityVersionAndHash> Versions { get; init; }

        /// <summary>
        /// Collection of disabled entity versions. The runtime will not permit disabled entity versions to be executed.
        /// </summary>
        [JsonPropertyName("disabled_versions")]
        public List<EntityVersion> DisabledVersions { get; init; }

        /// <summary>
        /// Mapping maintaining the set of URefs associated with each "user group". This can be used
        /// to control access to methods in a particular version of the entity. A method is callable
        /// by any context which "knows" any of the URefs associated with the method's user group.
        /// </summary>
        [JsonPropertyName("groups")]
        public List<NamedUserGroup> Groups { get; init; }

        /// <summary>
        /// The current state of the contract package.
        /// </summary>
        [JsonPropertyName("lock_status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LockStatus LockStatus { get; init; }

        public static Package FromContractPackage(ContractPackage contractPackage)
        {
            return new Package()
            {
                Versions = contractPackage.Versions.Select(v => new EntityVersionAndHash()
                    {
                        EntityVersion = new EntityVersion()
                            { Version = v.Version, ProtocolVersionMajor = v.ProtocolVersionMajor },
                        AddressableEntityHash = v.Hash,
                    }).ToList(),
                Groups = contractPackage.Groups.Select(g => new NamedUserGroup()
                    {
                        Name = g.Label,
                        Users = g.Keys
                    })
                    .ToList(),
                LockStatus = contractPackage.LockStatus,
            };
        }
    }
}
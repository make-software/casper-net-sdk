using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Information, entry points and named keys belonging to a Contract
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// Key to the storage of the ContractPackage object
        /// </summary>
        [JsonPropertyName("contract_package_hash")]
        public string ContractPackageHash { get; init; }

        /// <summary>
        /// Key to the storage of the ContractWasm object
        /// </summary>
        [JsonPropertyName("contract_wasm_hash")]
        public string ContractWasmHash { get; init; }

        /// <summary>
        /// List of entry points or methods in the contract.
        /// </summary>
        [JsonPropertyName("entry_points")]
        public List<EntryPoint> EntryPoints { get; init; }

        /// <summary>
        /// List of NamedKeys in te contract. 
        /// </summary>
        [JsonPropertyName("named_keys")]
        public List<NamedKey> NamedKeys { get; init; }

        /// <summary>
        /// The protocol version when the contract was deployed
        /// </summary>
        [JsonPropertyName("protocol_version")]
        public string ProtocolVersion { get; init; }
    }
}
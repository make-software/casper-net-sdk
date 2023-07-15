using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The raw bytes of the chainspec.toml, genesis accounts.toml, and global_state.toml files.
    /// </summary>
    public class ChainspecRawBytes
    {
        /// <summary>
        /// Hex-encoded raw bytes of the current chainspec.toml file.
        /// </summary>
        [JsonPropertyName("chainspec_bytes")]
        public string ChainspecBytes { get; init; }
        
        /// <summary>
        /// Hex-encoded raw bytes of the current genesis accounts.toml file.
        /// </summary>
        [JsonPropertyName("maybe_genesis_accounts_bytes")]
        public string MaybeGenesisAccountsBytes { get; init; }
        
        /// <summary>
        /// Hex-encoded raw bytes of the current global_state.toml file.
        /// </summary>
        [JsonPropertyName("maybe_global_state_bytes")]
        public string MaybeGlobalStateBytes { get; init; }

        /// <summary>
        /// The current chainspec.toml file of the node.
        /// </summary>
        [JsonIgnore]
        public string ChainspecAsString => Encoding.Default.GetString(Hex.Decode(ChainspecBytes));

        /// <summary>
        /// The current genesis accounts.toml file.
        /// </summary>
        [JsonIgnore]
        public string GenesisAccountsAsString => MaybeGenesisAccountsBytes != null
            ? Encoding.Default.GetString(Hex.Decode(MaybeGenesisAccountsBytes))
            : null;
        
        /// <summary>
        /// The current global_state.toml file.
        /// </summary>
        [JsonIgnore]
        public string GlobalStateAsString => MaybeGlobalStateBytes != null
            ? Encoding.Default.GetString(Hex.Decode(MaybeGlobalStateBytes))
            : null;
    }
}

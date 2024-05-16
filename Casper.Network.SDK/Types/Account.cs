using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Thresholds that have to be met when executing an action of a certain type.
    /// </summary>
    public class ActionThresholds
    {
        /// <summary>
        /// Threshold that has to be met for a deployment action
        /// </summary>
        [JsonPropertyName("deployment")]
        public uint Deployment { get; init; }

        /// <summary>
        /// Threshold that has to be met for a key management action
        /// </summary>
        [JsonPropertyName("key_management")]
        public uint KeyManagement { get; init; }

        /// <summary>
        /// Threshold for upgrading contracts.
        /// </summary>
        [JsonPropertyName("upgrade_management")]
        public uint UpgradeManagement { get; init; }
    }
    
    /// <summary>
    /// public key allowed to provide signatures on deploys for the account
    /// </summary>
    public class AssociatedKey
    {
        /// <summary>
        /// Hash derived from the public key
        /// </summary>
        [JsonPropertyName("account_hash")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey AccountHash { get; init; }

        /// <summary>
        /// Weight of the associated key 
        /// </summary>
        [JsonPropertyName("weight")]
        public uint Weight { get; init; }
    }
    
    /// <summary>
    /// Structure representing a user's account, stored in global state.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Account identity key
        /// </summary>
        [JsonPropertyName("account_hash")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey AccountHash { get; init; }

        /// <summary>
        /// Thresholds that have to be met when executing an action of a certain type.
        /// </summary>
        [JsonPropertyName("action_thresholds")]
        public ActionThresholds ActionThresholds { get; init; }

        /// <summary>
        /// Set of public keys allowed to provide signatures on deploys for the account
        /// </summary>
        [JsonPropertyName("associated_keys")]
        public List<AssociatedKey> AssociatedKeys { get; init; }

        /// <summary>
        /// Purse that can hold Casper tokens
        /// </summary>
        [JsonPropertyName("main_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef MainPurse { get; init; }

        /// <summary>
        /// Collection of named keys
        /// </summary>
        [JsonPropertyName("named_keys")]
        public List<NamedKey> NamedKeys { get; init; }
    }
}
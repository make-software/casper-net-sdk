using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The journal of execution transforms from an Era
    /// </summary>
    public class Step
    {
        /// <summary>
        /// The era in which the change occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The effect of executing the deploy.
        /// </summary>
        [JsonPropertyName("execution_effect")]
        public ExecutionEffect Effect { get; init; }
    }
}
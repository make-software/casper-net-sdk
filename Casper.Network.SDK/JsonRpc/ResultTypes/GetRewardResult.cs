using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for \"info_get_reward\" RPC response.
    /// </summary>
    public class GetRewardResult : RpcResult
    {
        /// <summary>
        /// The era for which the reward was calculated.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The reward amount.
        /// </summary>
        [JsonPropertyName("reward_amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// The delegation rate of the validator.
        /// </summary>
        [JsonPropertyName("delegation_rate")]
        public uint DelegationRate { get; init; }
    }
}
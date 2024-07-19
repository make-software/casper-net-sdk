using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The header portion of a TransactionV1.
    /// </summary>
    public class TransactionV1Header
    {
        /// <summary>
        /// The address of the initiator of a transaction.
        /// </summary>
        [JsonPropertyName("initiator_addr")]
        public InitiatorAddr InitiatorAddr { get; set; }

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
        /// Pricing mode of a Transaction.
        /// </summary>
        [JsonPropertyName("pricing_mode")]
        [JsonConverter(typeof(PricingMode.PricingModeConverter))]
        public IPricingMode PricingMode { get; set; }

        /// <summary>
        /// Hash of the body part of this Deploy.
        /// </summary>
        [JsonPropertyName("body_hash")]
        public string BodyHash { get; set; }

        /// <summary>
        /// Name of the chain where the deploy is executed.
        /// </summary>
        [JsonPropertyName("chain_name")]
        public string ChainName { get; set; }

        public TransactionV1Header()
        {
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow);
            Ttl = 1800000;
        }
    }
}
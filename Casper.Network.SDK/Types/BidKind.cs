using System.Numerics;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A bridge record pointing to a new `ValidatorBid` after the public key was changed.
    /// </summary>
    public class Bridge
    {
        
        /// <summary>
        /// Previous validator public key associated with the bid.
        /// </summary>
        [JsonPropertyName("old_validator_public_key")]
        public PublicKey OldValidator { get; init; }
        
        /// <summary>
        /// New validator public key associated with the bid.
        /// </summary>
        [JsonPropertyName("new_validator_public_key")]
        public PublicKey NewValidator { get; init; }
        
        /// <summary>
        /// Era when bridge record was created.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
    }
    /// <summary>
    /// Validator credit record.
    /// </summary>
    public class ValidatorCredit
    {
        /// <summary>
        /// The credit amount.
        /// </summary>
        [JsonPropertyName("amount")]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// The era id the credit was created.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// Validator public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        public PublicKey Validator { get; init; }
    }
    
    /// <summary>
    /// Auction bid variants.
    /// </summary>
    public class BidKind
    {
        /// <summary>
        /// A unified record indexed on validator data, with an embedded collection of all delegator bids assigned
        /// to that validator. The Unified variant is for legacy retrograde support, new instances will not be
        /// created going forward.
        /// </summary>
        [JsonPropertyName("Unified")]
        public Bid Unified { get; init; }
        
        /// <summary>
        /// A bid record containing only validator data.
        /// </summary>
        [JsonPropertyName("Validator")]
        public ValidatorBid Validator { get; init; }
        
        /// <summary>
        /// A bid record containing only delegator data.
        /// </summary>
        [JsonPropertyName("Delegator")]
        public Delegator Delegator { get; init; }
        
        /// <summary>
        /// A bridge record pointing to a new `ValidatorBid` after the public key was changed.
        /// </summary>
        [JsonPropertyName("Bridge")]
        public Bridge Bridge { get; init; }
        
        /// <summary>
        /// Credited amount.
        /// </summary>
        [JsonPropertyName("Credit")]
        public ValidatorCredit Credit { get; init; }
    }
}

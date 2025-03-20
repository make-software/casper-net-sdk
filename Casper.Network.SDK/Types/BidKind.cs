using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

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
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey OldValidator { get; init; }
        
        /// <summary>
        /// New validator public key associated with the bid.
        /// </summary>
        [JsonPropertyName("new_validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
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
        [JsonConverter(typeof(BigIntegerConverter))]
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
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Validator { get; init; }
    }

    public class UnbondKind
    {
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Validator { get; init; }
        
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey DelegatedPublicKey { get; init; }
        
        public string DelegatedPurse { get; init; }
    }

    public class UnbondEra
    {
        /// <summary>
        /// The purse that was used for bonding.
        /// </summary>
        [JsonPropertyName("bonding_purse")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef BondingPurse { get; init; }
        
        /// <summary>
        /// Era in which this unbonding request was created.
        /// </summary>
        [JsonPropertyName("era_of_creation")]
        public ulong EraOfCreation { get; init; }
        
        /// <summary>
        /// Unbonding Amount
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// The validator public key to re-delegate to.
        /// </summary>
        [JsonPropertyName("new_validator")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey NewValidator { get; init; }
    }
    
    
    public class Unbond
    {
        /// <summary>
        /// Validator public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey Validator { get; init; }

        /// <summary>
        /// Unbond kind.
        /// </summary>
        [JsonPropertyName("unbond_kind")]
        public UnbondKind Kind { get; init; }
        
        [JsonPropertyName("eras")]
        public List<UnbondEra> Eras { get; init; }
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
        public DelegatorBid Delegator { get; init; }
        
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
        
        /// <summary>
        /// Represents a validator reserving a slot for specific delegator"
        /// </summary>
        [JsonPropertyName("Reservation")]
        public Reservation Reservation { get; init; }
        
        /// <summary>
        /// A bid record containing Unbond information
        /// </summary>
        [JsonPropertyName("Unbond")]
        public Unbond Unbond { get; init; }
    }
}

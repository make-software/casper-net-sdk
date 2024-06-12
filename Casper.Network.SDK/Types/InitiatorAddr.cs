using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The address of the initiator of a TransactionV1
    /// </summary>
    public class InitiatorAddr
    {
        /// <summary>
        /// The public key of the initiator
        /// </summary>
        [JsonPropertyName("PublicKey")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }
        
        /// <summary>
        /// The account hash derived from the public key of the initiator
        /// </summary>
        [JsonPropertyName("AccountHash")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey AccountHash { get; init; }

        public InitiatorAddr()
        {
        }
        
        public InitiatorAddr(PublicKey publicKey)
        {
            this.PublicKey = publicKey;
        }
        
        public InitiatorAddr(AccountHashKey accountHash)
        {
            this.AccountHash = accountHash;
        }
        
        public override string ToString()
        {
            return PublicKey != null
                ? PublicKey.ToString()
                : (AccountHash != null
                    ? AccountHash.ToString()
                    : null);
        }
        
        public string ToHexString()
        {
            return PublicKey != null
                ? PublicKey.ToString()
                : (AccountHash != null
                    ? AccountHash.ToHexString()
                    : null);
        }
    }
}

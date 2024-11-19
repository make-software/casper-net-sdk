using System;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;

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

        public static InitiatorAddr FromPublicKey(PublicKey publicKey)
        {
            return new InitiatorAddr(publicKey);
        }
        
        public static InitiatorAddr FromAccountHash(AccountHashKey accountHashKey)
        {
            return new InitiatorAddr(accountHashKey);
        }
        
        public override string ToString()
        {
            return PublicKey != null
                ? PublicKey.ToString()
                : AccountHash?.ToString();
        }
        
        public string ToHexString()
        {
            return PublicKey != null
                ? PublicKey.ToString()
                : AccountHash?.ToHexString();
        }

        const ushort TAG_FIELD_INDEX = 0;
        const byte PUBLIC_KEY_VARIANT_TAG = 0;
        const ushort PUBLIC_KEY_FIELD_INDEX = 1;
        const byte ACCOUNT_HASH_VARIANT_TAG = 1;
        const ushort ACCOUNT_HASH_FIELD_INDEX = 1;
            
        public byte[] ToBytes()
        {
            if(PublicKey != null)
                return new CalltableSerialization()
                    .AddField(TAG_FIELD_INDEX, new byte[] { PUBLIC_KEY_VARIANT_TAG })
                    .AddField(PUBLIC_KEY_FIELD_INDEX, CLValue.PublicKey(PublicKey))
                    .GetBytes();
                    
            if(AccountHash != null)
                return new CalltableSerialization()
                    .AddField(TAG_FIELD_INDEX, new byte[] { ACCOUNT_HASH_VARIANT_TAG })
                    .AddField(PUBLIC_KEY_FIELD_INDEX, CLValue.Key(AccountHash))
                    .GetBytes();
            
            throw new Exception("Unable to serialize initiator addr");
        }
    }
}


using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Auction bid variants. Kinds of delegation bids.
    /// </summary>
    public class DelegatorKind
    {
        /// <summary>
        /// Delegation from public key.
        /// </summary>
        [JsonPropertyName("PublicKey")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }
        
        /// <summary>
        /// Delegation from purse.
        /// </summary>
        [JsonPropertyName("Purse")]
        public string Purse { get; init; }

        public byte[] ToBytes()
        {
            var ms = new MemoryStream();
            if (PublicKey != null)
            {
                ms.WriteByte(0x00);
                ms.Write(this.PublicKey.GetBytes());
                return ms.ToArray();
            }
            else // Purse
            {
                ms.WriteByte(0x01);
                ms.Write(Hex.Decode(Purse));
                return ms.ToArray();
            }
        }

        public CLValue ToCLValue()
        {
            return new CLValue(ToBytes(), CLType.Any);
        }
    }
}

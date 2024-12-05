using System.IO;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Represents a validator reserving a slot for specific delegator
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// The validator public key.
        /// </summary>
        [JsonPropertyName("validator_public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey ValidatorPublicKey { get; init; }

        /// <summary>
        /// The delegator public key or purse.
        /// </summary>
        [JsonPropertyName("delegator_kind")]
        public DelegatorKind DelegatorKind { get; init; }
        
        /// <summary>
        /// The delegation rate.
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("delegation_rate")]
        public uint DelegationRate { get; init; }

        public byte[] ToBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(this.DelegatorKind.ToBytes());
            ms.Write(ValidatorPublicKey.GetBytes());
            ms.WriteByte((byte)DelegationRate);
            return ms.ToArray();
        }

        public CLValue ToCLValue()
        {
            return new CLValue(ToBytes(), CLType.Any);
        }
    }
}

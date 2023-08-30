using System;
using System.IO;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Unforgeable Reference. This key type is used for storing any type of value
    /// except Account. Additionally, URefs used in contracts carry permission information
    /// to prevent unauthorized usage of the value stored under the key.
    /// </summary>
    public class URef : GlobalStateKey
    {
        public static string KEYPREFIX = "uref-";

        public AccessRights AccessRights { get; }

        public URef(string value) : base(value)
        {
            KeyIdentifier = KeyIdentifier.URef;

            if (!value.StartsWith(KEYPREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{KEYPREFIX}'.",
                    nameof(value));
            
            var parts = value.Substring(5).Split(new char[] {'-'});
            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An URef object must end with an access rights suffix.");
            if (parts[0].Length != 64)
                throw new ArgumentOutOfRangeException(nameof(value), "An Uref object must contain a 32 byte value.");
            if (parts[1].Length != 3)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An URef object must contain a 3 digits access rights suffix.");

            CEP57Checksum.Decode(parts[0], out int checksumResult);
            if (checksumResult == CEP57Checksum.InvalidChecksum)
                throw new ArgumentException("URef checksum mismatch.");
            
            AccessRights = (AccessRights) uint.Parse(parts[1]);
        }
        
        /// <summary>
        /// Creates an URef from a 33 bytes array. Last byte corresponds to the access rights.
        /// </summary>
        public URef(byte[] bytes)
            : this($"{KEYPREFIX}{Hex.ToHexString(bytes.Slice(0,32))}-{(int)bytes[32]:000}")
        {
        }
        
        /// <summary>
        /// Creates an URef from a 32 bytes array and the access rights.
        /// </summary>
        public URef(byte[] rawBytes, AccessRights accessRights)
            : this($"{KEYPREFIX}{Hex.ToHexString(rawBytes)}-{(int)accessRights:000}")
        {
        }

        protected override byte[] _GetRawBytesFromKey(string key)
        {
            key = key.Substring(0, key.LastIndexOf('-'));
            return Hex.Decode(key.Substring(key.LastIndexOf('-')+1));
        }
        
        public override byte[] GetBytes()
        {
            var ms = new MemoryStream(34);
            ms.WriteByte((byte)this.KeyIdentifier);
            ms.Write(this.RawBytes);
            ms.WriteByte((byte)this.AccessRights);
            
            return ms.ToArray();
        }

        public override string ToString()
        {
            return KEYPREFIX + CEP57Checksum.Encode(RawBytes) + $"-{(byte) AccessRights:000}";
        }
    }
}

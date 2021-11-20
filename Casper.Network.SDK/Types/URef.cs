using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class URef : GlobalStateKey
    {
        public AccessRights AccessRights { get; }
        
        public byte[] RawBytes { get; }

        public URef(string value) : base(value, "uref-")
        {
            var parts = value.Substring(5).Split(new char[] {'-'});
            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An Uref object must end with an access rights suffix.");
            if (parts[0].Length != 64)
                throw new ArgumentOutOfRangeException(nameof(value), "An Uref object must contain a 32 byte value.");
            if (parts[1].Length != 3)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An Uref object must contain a 3 digits access rights suffix.");

            RawBytes = Hex.Decode(parts[0]);
            AccessRights = (AccessRights) uint.Parse(parts[1]);
        }
        
        public URef(byte[] rawBytes, AccessRights accessRights)
            : base($"uref-{Hex.ToHexString(rawBytes)}-{(int)accessRights:000)}", "uref-")
        {
            RawBytes = rawBytes;
            AccessRights = accessRights;
        }

        public override string ToString()
        {
            return "uref-" + Hex.ToHexString(RawBytes) + $"-{(byte) AccessRights:000}";
        }
    }
}
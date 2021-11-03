using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperSDK.Types
{
    public class URef
    {
        // ex: "uref-e65e72d685359dd21b1a7d7eff2862895c556067499ae6524e67a0e229ceb49e-007"
        public AccessRights AccessRights { get; }
        public byte[] RawBytes { get; }

        protected URef(byte[] rawBytes, AccessRights accessRights)
        {
            RawBytes = rawBytes;
            AccessRights = accessRights;
        }

        public override string ToString()
        {
            return "uref-" + Hex.ToHexString(RawBytes) + $"-{(byte)AccessRights:000}";
        }  

        public static URef FromRawBytes(byte[] rawBytes, AccessRights accessRights)
        {
            return new URef(rawBytes, accessRights);
        }

        public static URef FromString(string value)
        {
            if (!value.StartsWith("uref-"))
                throw new ArgumentOutOfRangeException(nameof(value), "An URef object must start with 'uref-'.");

            var parts = value.Substring(5).Split(new char[] {'-'});
            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An Uref object must end with an access rights suffix.");
            if (parts[0].Length != 64)
                throw new ArgumentOutOfRangeException(nameof(value), "An Uref object must contain a 32 byte value.");
            if (parts[1].Length != 3)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An Uref object must contain a 3 digits access rights suffix.");

            return new URef(Hex.Decode(parts[0]), (AccessRights) uint.Parse(parts[1]));
        }
    }
}
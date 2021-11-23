using System.IO;
using System.Numerics;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Utils
{
    public static class BinaryReaderExtensions
    {
        public static BigInteger ReadCLBigInteger(this BinaryReader reader)
        {
            var length = (int)reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            return new BigInteger(bytes);
        }

        public static string ReadCLString(this BinaryReader reader)
        {
            var length = (int) reader.ReadInt32();
            var bytes = reader.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static URef ReadCLURef(this BinaryReader reader)
        {
            return new URef(reader.ReadBytes(33));
        }

        public static PublicKey ReadCLPublicKey(this BinaryReader reader)
        {
            int keyAlgo = reader.PeekChar();
            return PublicKey.FromBytes(reader.ReadBytes(keyAlgo == 0x01 ? KeyAlgo.ED25519.GetKeySizeInBytes() 
                : KeyAlgo.SECP256K1.GetKeySizeInBytes()));
        }

        public static GlobalStateKey ReadCLGlobalStateKey(this BinaryReader reader)
        {
            int keyId = reader.PeekChar();
            
            // Era Info serializes as a u64 (8 bytes + 1 tag byte)
            if (keyId == (char)KeyIdentifier.EraInfo) 
                return GlobalStateKey.FromBytes(reader.ReadBytes(9));
            
            //URef serializes as 33 bytes + 1 tag byte
            if (keyId == (char) KeyIdentifier.URef)
                return GlobalStateKey.FromBytes(reader.ReadBytes(34));
            
            // all others serialize as 32 bytes + 1 tag byte
            return GlobalStateKey.FromBytes(reader.ReadBytes(33));
        }
    }
}
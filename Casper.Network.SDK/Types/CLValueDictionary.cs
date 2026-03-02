using System.IO;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Represents a parsed CLValue dictionary entry containing a raw value, a seed URef, and an item key string.
    /// </summary>
    public class CLValueDictionary
    {
        /// <summary>
        /// The raw value bytes of the dictionary entry.
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// The seed URef associated with the dictionary.
        /// </summary>
        public URef Seed { get; }

        /// <summary>
        /// The string key identifying the item in the dictionary.
        /// </summary>
        public string ItemKey { get; }

        private CLValueDictionary(byte[] value, URef seed, string itemKey)
        {
            Value = value;
            Seed = seed;
            ItemKey = itemKey;
        }

        /// <summary>
        /// Parses a byte array into a <see cref="CLValueDictionary"/> instance.
        /// </summary>
        /// <param name="bytes">The raw bytes to parse.</param>
        /// <returns>A new <see cref="CLValueDictionary"/> instance.</returns>
        public static CLValueDictionary Parse(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            // (2) Read uint32 length of the value bytes
            var valueLength = reader.ReadCLU32();

            // (3) Read the value bytes
            var serializer = new CLValueByteSerializer();
            var value = serializer.FromReader(reader);
            
            // (4) Read the URef key tag byte (KeyIdentifier.URef = 0x0e)
            // reader.ReadByte();

            // (5) Read the access rights byte, then the seed length, then the seed bytes.
            //     Create a URef with the seed bytes and permissions '0'.
            // var accessRights = (AccessRights)reader.ReadByte();
            var seedLength = reader.ReadCLU32();
            var seedBytes = reader.ReadBytes((int)seedLength);
            var seed = new URef(seedBytes, AccessRights.NONE);

            // Read a CLValue string (u32 length prefix + UTF-8 bytes)
            var itemKey = reader.ReadCLString();

            return new CLValueDictionary(value.Bytes, seed, itemKey);
        }
    }
}


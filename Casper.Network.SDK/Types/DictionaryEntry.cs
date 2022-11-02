using System.IO;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class DictionaryEntry
    {
        public URef DictionarySeed { get; init; }
        
        public CLValue Value { get; init; }
        
        public CLValue ItemKey { get; init; }

        public static DictionaryEntry FromBytes(byte[] bytes)
        {
            // a dictionary entry in execution results is serialized as three objects:
            // - 1. a CLValue that stores the entry value
            // - 2. a Byte array with the dictionary seed hash
            // - 3. a string with the dictionary item key
            
            // first, read the Any-CLValue and decode the inner CLValue
            //
            var serializer = new CLValueByteSerializer();
            var ms = new MemoryStream(bytes);
            var value = serializer.FromBytes(ms);
            
            var reader = new BinaryReader(ms);
            // var o = reader.ReadCLItem(value.TypeInfo, null);
            
            // second, read the dictionary seed hash
            //
            var hashLength = reader.ReadCLI32();
            var hash = reader.ReadBytes(hashLength);
            
            // third, read the dictionary item key
            var str = reader.ReadCLString();

            return new DictionaryEntry()
            {
                DictionarySeed = new URef(hash, AccessRights.READ_ADD_WRITE),
                Value = value,
                ItemKey = str
            };
        }
    }
}

using System;
using System.IO;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.ByteSerializers
{
    public class NamedArgByteSerializer : BaseByteSerializer, IByteSerializer<NamedArg>
    {
        public byte[] ToBytes(NamedArg source)
        {
            var ms = new MemoryStream();

            var bName = System.Text.Encoding.UTF8.GetBytes(source.Name);
            WriteInteger(ms, bName.Length);
            WriteBytes(ms, bName);

            var valueSerializer = new CLValueByteSerializer();
            WriteBytes(ms, valueSerializer.ToBytes(source.Value));

            return ms.ToArray();
        }

        public NamedArg FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            return FromReader(reader);
        }
        
        public NamedArg FromReader(BinaryReader reader)
        {
            var name = reader.ReadCLString();
            var clValue = new CLValueByteSerializer().FromReader(reader);
            return new NamedArg(name, clValue);
        }
    }
}
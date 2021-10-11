using System;
using System.IO;
using NetCasperSDK.Types;

namespace NetCasperSDK.ByteSerializers
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
    }
}
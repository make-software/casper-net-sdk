using System;
using System.IO;
using NetCasperSDK.Types;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace NetCasperSDK.ByteSerializers
{
    public class CLValueByteSerializer : BaseByteSerializer, IByteSerializer<CLValue>
    {
        public byte[] ToBytes(CLValue source)
        {
            var ms = new MemoryStream();
            // serialize data length (4 bytes)
            //
            WriteInteger(ms, source.Bytes.Length);
            // serialize data
            //
            WriteBytes(ms, source.Bytes);
            // serialize type
            //
            WriteByte(ms, (byte)source.TypeInfo.Type);
            // serialize inner types (if any)
            //
            if (source.TypeInfo is CLOptionTypeInfo option)
                if(option.OptionType != null)
                    WriteByte(ms, (byte)option.OptionType.Type);
            if (source.TypeInfo is CLListTypeInfo list)
                WriteByte(ms, (byte)list.ListType.Type);
            if(source.TypeInfo is CLByteArrayTypeInfo ba)
                WriteInteger(ms, ba.Size);
            if (source.TypeInfo is CLTuple1TypeInfo tuple1)
                WriteByte(ms, (byte)tuple1.Type0.Type);
            if (source.TypeInfo is CLTuple2TypeInfo tuple2)
            {
                WriteByte(ms, (byte)tuple2.Type0.Type);
                WriteByte(ms, (byte)tuple2.Type1.Type);
            }
            if (source.TypeInfo is CLTuple3TypeInfo tuple3)
            {
                WriteByte(ms, (byte)tuple3.Type0.Type);
                WriteByte(ms, (byte)tuple3.Type1.Type);
                WriteByte(ms, (byte)tuple3.Type2.Type);
            }
            return ms.ToArray();
        }
    }
}
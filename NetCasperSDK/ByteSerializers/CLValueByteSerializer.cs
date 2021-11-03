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
            // serialize type and inner types (if any) recursively 
            //
            CLTypeToBytes(ms, source.TypeInfo);

            return ms.ToArray();
        }

        private void CLTypeToBytes(MemoryStream ms, CLTypeInfo innerType)
        {
            WriteByte(ms, (byte)innerType.Type);

            switch (innerType)
            {
                case CLOptionTypeInfo option:
                    if(option.OptionType != null)
                        CLTypeToBytes(ms, option.OptionType);
                    break;
                case CLListTypeInfo list:
                    CLTypeToBytes(ms, list.ListType);
                    break;
                case CLByteArrayTypeInfo ba:
                    WriteInteger(ms, ba.Size);
                    break;
                case CLResultTypeInfo result:
                    CLTypeToBytes(ms, result.Ok);
                    CLTypeToBytes(ms, result.Err);
                    break;
                case CLMapTypeInfo map:
                    CLTypeToBytes(ms, map.KeyType);
                    CLTypeToBytes(ms, map.ValueType);
                    break;
                case CLTuple1TypeInfo tuple1:
                    CLTypeToBytes(ms, tuple1.Type0);
                    break;
                case CLTuple2TypeInfo tuple2:
                    CLTypeToBytes(ms, tuple2.Type0);
                    CLTypeToBytes(ms, tuple2.Type1);
                    break;
                case CLTuple3TypeInfo tuple3:
                    CLTypeToBytes(ms, tuple3.Type0);
                    CLTypeToBytes(ms, tuple3.Type1);
                    CLTypeToBytes(ms, tuple3.Type2);
                    break;
            }
        }
    }
}
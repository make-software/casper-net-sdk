using System;
using System.IO;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace Casper.Network.SDK.ByteSerializers
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

        public CLValue FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            return FromReader(reader);
        }
        
        public CLValue FromReader(BinaryReader reader)
        {
            // read data length and data bytes
            var dataLength = reader.ReadCLU32();
            var dataBytes = reader.ReadBytes((int)dataLength);

            // read type info recursively
            var typeInfo = CLTypeFromBytes(reader);

            return new CLValue(dataBytes, typeInfo);
        }

        private CLTypeInfo CLTypeFromBytes(BinaryReader reader)
        {
            var tag = (CLType)reader.ReadByte();

            return tag switch
            {
                CLType.Option    => new CLOptionTypeInfo(CLTypeFromBytes(reader)),
                CLType.List      => new CLListTypeInfo(CLTypeFromBytes(reader)),
                CLType.ByteArray => new CLByteArrayTypeInfo(reader.ReadCLI32()),
                CLType.Result    => new CLResultTypeInfo(CLTypeFromBytes(reader), CLTypeFromBytes(reader)),
                CLType.Map       => new CLMapTypeInfo(CLTypeFromBytes(reader), CLTypeFromBytes(reader)),
                CLType.Tuple1    => new CLTuple1TypeInfo(CLTypeFromBytes(reader)),
                CLType.Tuple2    => new CLTuple2TypeInfo(CLTypeFromBytes(reader), CLTypeFromBytes(reader)),
                CLType.Tuple3    => new CLTuple3TypeInfo(CLTypeFromBytes(reader), CLTypeFromBytes(reader), CLTypeFromBytes(reader)),
                _                => new CLTypeInfo(tag)
            };
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
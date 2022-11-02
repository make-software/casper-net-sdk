using System;
using System.IO;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.ByteSerializers
{
    public class BaseByteSerializer
    {
        protected static void WriteInteger(MemoryStream ms, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            ms.Write(bytes);
        }
        
        protected static void WriteUInteger(MemoryStream ms, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            ms.Write(bytes);
        }
     
        protected static void WriteULong(MemoryStream ms, ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            ms.Write(bytes);
        }
        
        protected static void WriteByte(MemoryStream ms, byte value)
        {
            ms.WriteByte(value);
        }
        
        protected static void WriteBytes(MemoryStream ms, byte[] value)
        {
            ms.Write(value);
        }
        
        protected static void WriteString(MemoryStream ms, string value)
        {
            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var lenBytes = BitConverter.GetBytes(valueBytes.Length);
            if(!BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
            ms.Write(lenBytes);
            ms.Write(valueBytes);
        }

        protected static bool ReadInteger(MemoryStream ms, out int value)
        {
            value = 0;
            var bytes = new byte[sizeof(int)];
            if(ms.Read(bytes, 0, sizeof(int)) != sizeof(int)) return false;
            value = BitConverter.ToInt32(bytes, 0);
            return true;
        }

        protected static bool ReadUInteger(MemoryStream ms, out uint value)
        {
            value = 0;
            var bytes = new byte[sizeof(uint)];
            if(ms.Read(bytes, 0, sizeof(uint)) != sizeof(uint)) return false;
            value = BitConverter.ToUInt32(bytes, 0);
            return true;
        }

        protected static bool ReadLong(MemoryStream ms, out long value)
        {
            value = 0;
            var bytes = new byte[sizeof(long)];
            if(ms.Read(bytes, 0, sizeof(long)) != sizeof(long)) return false;
            value = BitConverter.ToInt64(bytes, 0);
            return true;
        }

        protected static bool ReadULong(MemoryStream ms, out ulong value)
        {
            value = 0;
            var bytes = new byte[sizeof(ulong)];
            if(ms.Read(bytes, 0, sizeof(ulong)) != sizeof(ulong)) return false;
            value = BitConverter.ToUInt32(bytes, 0);
            return true;
        }

        protected static bool ReadByte(MemoryStream ms, out byte value)
        {
            value = 0;
            var bytes = new byte[sizeof(byte)];
            if(ms.Read(bytes, 0, sizeof(byte)) != sizeof(byte)) return false;
            value = bytes[0];
            return true;
        }

        protected static bool ReadCLTypeInfo(MemoryStream ms, out CLTypeInfo value)
        {
            value = null;

            if (!ReadByte(ms, out var type))
                return false;

            if (!Enum.IsDefined(typeof(CLType), type))
                return false;
            
            var outerType = (CLType)type;
            switch (outerType)
            {
                case CLType.Option:
                    if (!ReadCLTypeInfo(ms, out var optionInnerType))
                        return false;
                    value = new CLOptionTypeInfo(optionInnerType);
                    break;
                case CLType.List:
                    if (!ReadCLTypeInfo(ms, out var listInnerType))
                        return false;
                    value = new CLOptionTypeInfo(listInnerType);
                    break;
                case CLType.ByteArray:
                    if (!ReadInteger(ms, out var byteArraySize))
                        return false;
                    value = new CLByteArrayTypeInfo(byteArraySize);
                    break;
                case CLType.Result:
                    if (!ReadCLTypeInfo(ms, out var okInnerType))
                        return false;
                    if (!ReadCLTypeInfo(ms, out var errInnerType))
                        return false;
                    value = new CLResultTypeInfo(okInnerType, errInnerType);
                    break;
                case CLType.Map:
                    if (!ReadCLTypeInfo(ms, out var keyInnerType))
                        return false;
                    if (!ReadCLTypeInfo(ms, out var valueInnerType))
                        return false;
                    value = new CLMapTypeInfo(keyInnerType, valueInnerType);
                    break;
                case CLType.Tuple1:
                    if (!ReadCLTypeInfo(ms, out var tuple1InnerType1))
                        return false;
                    value = new CLTuple1TypeInfo(tuple1InnerType1);
                    break;
                case CLType.Tuple2:
                    if (!ReadCLTypeInfo(ms, out var tuple2InnerType1))
                        return false;
                    if (!ReadCLTypeInfo(ms, out var tuple2InnerType2))
                        return false;
                    value = new CLTuple2TypeInfo(tuple2InnerType1, tuple2InnerType2);
                    break;
                case CLType.Tuple3:
                    if (!ReadCLTypeInfo(ms, out var tuple3InnerType1))
                        return false;
                    if (!ReadCLTypeInfo(ms, out var tuple3InnerType2))
                        return false;
                    if (!ReadCLTypeInfo(ms, out var tuple3InnerType3))
                        return false;
                    value = new CLTuple3TypeInfo(tuple3InnerType1, tuple3InnerType2, tuple3InnerType3);
                    break; 
                default:
                    value = new CLTypeInfo(outerType);
                    break;
            }

            return true;
        }
    }
}

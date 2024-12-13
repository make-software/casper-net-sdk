using System;
using System.IO;

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
        
        protected static void WriteUShort(MemoryStream ms, ushort value)
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
        
        protected static void WriteMaybeUInteger(MemoryStream ms, uint? maybeValue)
        {
            WriteByte(ms, (byte)(maybeValue.HasValue ? 0x01 : 0x00));

            if (maybeValue.HasValue)
            {
                var bytes = BitConverter.GetBytes(maybeValue.Value);
                if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
                ms.Write(bytes);
            }
        }
    }
}
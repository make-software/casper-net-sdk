using System;

namespace Casper.Network.SDK.ByteSerializers
{
    public class LittleEndianConverter
    {
        public static byte[] GetBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static int ToInt32(byte[] bytes, int startIndex = 0)
        {
            if (!BitConverter.IsLittleEndian)
            {
                var temp = new byte[4];
                Array.Copy(bytes, startIndex, temp, 0, 4);
                Array.Reverse(temp); // Convert to native order
                return BitConverter.ToInt32(temp, 0);
            }
            return BitConverter.ToInt32(bytes, startIndex);
        }
    }
}
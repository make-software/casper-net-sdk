#if NETSTANDARD2_0

using System;

public static class BitConverterExtensions
{
    public static Int64 ToInt64(byte[] value, int startIndex = 0)
    {
        return BitConverter.ToInt64(value, startIndex);
    }

    public static Int32 ToInt32(byte[] value, int startIndex = 0)
    {
        return BitConverter.ToInt32(value, startIndex);
    }

    public static UInt64 ToUInt64(byte[] value, int startIndex = 0)
    {
        return BitConverter.ToUInt64(value, startIndex);
    }

    public static UInt32 ToUInt32(byte[] value, int startIndex = 0)
    {
        return BitConverter.ToUInt32(value, startIndex);
    }
}

#endif

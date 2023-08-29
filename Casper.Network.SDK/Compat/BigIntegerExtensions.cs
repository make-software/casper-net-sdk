#if NETSTANDARD2_0

using System;
using System.Numerics;

public static class BigIntegerExtensions
{
    public static byte[] ToByteArray(this BigInteger value, bool isUnsigned = false)
    {
        byte[] bytes = value.ToByteArray();

        if (isUnsigned)
        {
            if (value < 0)
            {
                throw new InvalidOperationException("Cannot retrieve an unsigned byte array representation of a negative BigInteger.");
            }

            // If the BigInteger is positive and the highest byte is 0x80 or higher,
            // an additional byte is added to indicate a positive number when using two's complement notation.
            // For the unsigned representation, this additional byte is not necessary.
            if (bytes.Length > 1 && bytes[bytes.Length - 1] == 0)
            {
                Array.Resize(ref bytes, bytes.Length - 1);
            }
        }

        return bytes;
    }
}

#endif

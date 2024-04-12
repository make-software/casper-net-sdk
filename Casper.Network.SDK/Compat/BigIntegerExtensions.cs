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

    public static int GetBitLength(this BigInteger value)
    {
        // If the value is zero, its bit length is 0
        if (value == BigInteger.Zero)
            return 0;

        // Get the byte array without the sign
        byte[] bytes = value.ToByteArray();

        // Find the highest-order byte with a non-zero value
        int highestByte = bytes.Length - 1;
        while (highestByte > 0 && bytes[highestByte] == 0)
        {
            highestByte--;
        }

        // Count the bits in the last used byte
        int bitsInLastByte = 0;
        byte lastByte = bytes[highestByte];
        while (lastByte != 0)
        {
            bitsInLastByte++;
            lastByte >>= 1;
        }

        // Calculate the total bit length
        // (Number of full bytes minus 1) * 8 + bits in last byte
        return highestByte * 8 + bitsInLastByte;
    }
}

#endif

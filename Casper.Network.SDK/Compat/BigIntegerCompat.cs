using System;
using System.Numerics;

public class BigIntegerCompat
{
    public static BigInteger Create(byte[] value, bool isUnsigned = false, bool isBigEndian = false)
    {
#if NET7_0_OR_GREATER
        return new BigInteger(value, true, false);
#elif NETSTANDARD2_0        
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (isBigEndian)
        {
            Array.Reverse(value);
        }

        if (isUnsigned)
        {
            // Ensure that the resulting BigInteger is treated as an unsigned value by appending a 0 byte if necessary
            if (value[value.Length - 1] >= 0x80)
            {
                Array.Resize(ref value, value.Length + 1);
                value[value.Length - 1] = 0;
            }
        }

        return new BigInteger(value);
#endif
    }
}


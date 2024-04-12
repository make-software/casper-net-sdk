
using System;

public static class BytesExtensions
{
    public static byte[] Slice(this byte[] bytes, int from, int? to = null)
    {
        int actualTo = to ?? bytes.Length;  // if to is not provided, use the last index

        if (from < 0 || actualTo > bytes.Length || from > actualTo)
        {
            throw new IndexOutOfRangeException();
        }

        int length = actualTo - from;
        byte[] result = new byte[length];

        Array.Copy(bytes, from, result, 0, length);

        return result;
    }
}


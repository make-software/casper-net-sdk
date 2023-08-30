#if NETSTANDARD2_0

using System.IO;

public static class MemoryStreamExtensions
{
    public static void Write(this MemoryStream stream, byte[] buffer)
    {
        stream.Write(buffer, 0, buffer.Length);
    }
}

#endif

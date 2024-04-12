using System.IO;
using System.Threading.Tasks;

public static class FileExtensions
{
    public static async Task<byte[]> ReadAllBytesAsync(string path)
    {
#if NET7_0_OR_GREATER
        return await File.ReadAllBytesAsync(path);
#elif NETSTANDARD2_0

        // Using FileStream with asynchronous flag
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                   FileOptions.Asynchronous))
        {
            // Allocate memory for the file's content
            byte[] bytes = new byte[stream.Length];
            // Read the entire file asynchronously
            int numBytesToRead = (int)stream.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = await stream.ReadAsync(bytes, numBytesRead, numBytesToRead);
                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                numBytesRead += n;
                numBytesToRead -= n;
            }

            return bytes;
        }
#endif
    }
}


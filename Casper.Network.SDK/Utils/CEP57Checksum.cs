using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Utils
{
    /// <summary>
    /// CEP 0057 introduces a checksum for hex-encoded values based on the capitalization
    /// of letters in an hexadecimal string. 
    /// </summary>
    /// <seealso href="https://github.com/casper-network/ceps/blob/master/text/0057-checksummed-addresses.md">CEP 0057 Checksummed Addresses</seealso>
    public class CEP57Checksum
    {
        private const int SMALL_BYTES_COUNT = 75;
        
        public const int NoChecksum = 0x00;
        public const int ValidChecksum = 0x01;
        public const int InvalidChecksum = 0x02;
        
        private static byte[] _bytes_to_nibbles(byte[] bytes)
        {
            var nibbles = new byte[bytes.Length * 2];

            try
            {
                var writer = new BinaryWriter(new MemoryStream(nibbles));
                foreach (var b in bytes)
                {
                    writer.Write((byte) (b >> 4));
                    writer.Write((byte) (b & 0x0F));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return nibbles;
        }

        private static bool[] _bytes_to_bits_cycle(byte[] bytes)
        {
            var bits = new bool[bytes.Length * 8];
            for (int i = 0, k = 0; i < bytes.Length; i++)
            for (int j = 0; j < 8; j++)
                bits[k++] = ((bytes[i] >> j) & 0x01) == 0x01;

            return bits;
        }

        private static readonly char[] HexChars =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
        };

        /// <summary>
        /// Returns true if the input hexadecimal string includes the CEP57 checksum.
        /// An string includes checksum when letters are mixed-cased.
        /// </summary>
        /// <exception cref="ArgumentException">This method trhows an exception if the string is not hexadecimal.</exception>
        public static bool HasChecksum(string hex)
        {
            int mix = 0;
            foreach (var c in hex.ToCharArray())
            {
                if (c >= '0' && c <= '9')
                    mix |= 0x00;
                else if (c >= 'a' && c <= 'f')
                    mix |= 0x01;
                else if (c >= 'A' && c <= 'F')
                    mix |= 0x02;
                else
                    throw new ArgumentException("Input is not an hexadecimal string", nameof(hex));
            }

            return mix > 2;
        }

        /// <summary>
        /// Encodes an array of bytes into an hexadecimal string with CEP57 checksum.
        /// Arrays longer than 75 bytes do not include the checksum.
        /// </summary>
        public static string Encode(byte[] input)
        {
            if (input.Length > SMALL_BYTES_COUNT)
                return Hex.ToHexString(input);
            
            var nibbles = _bytes_to_nibbles(input);

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            bcBl2bdigest.BlockUpdate(input, 0, input.Length);
            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            var hashBits = _bytes_to_bits_cycle(hash);
            var sb = new StringBuilder(nibbles.Length);

            int k = 0;
            foreach (var n in nibbles)
            {
                var c = HexChars[n];
                if ((c >= 'a' && c <= 'f') && hashBits[k++])
                    sb.Append((char) (c - ('a' - 'A')));
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts an hexadecimal string to an array of bytes.
        /// If the string contains a valid checksum, `checksumResult` returns `CEP57Checksum.ValidChecksum`.
        /// If the string contains an invalid checksum, `checksumResult`returns `CEP57Checksum.InvalidChecksum`.
        /// `checkumResult`returns `CEP57Checksum.NoChecksum` if the string does not include CEP57 checksum.
        /// </summary>
        public static byte[] Decode(string hex, out int checksumResult)
        {
            var bytes = Hex.Decode(hex);

            if (bytes.Length > SMALL_BYTES_COUNT || !HasChecksum(hex))
            {
                checksumResult = NoChecksum;
                return bytes;
            }

            var computed = Encode(bytes);
            
            checksumResult = computed.Equals(hex) ? ValidChecksum : InvalidChecksum; 
            
            return bytes;
        }
    }
}
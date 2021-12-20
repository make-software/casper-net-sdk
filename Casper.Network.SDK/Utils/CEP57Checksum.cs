using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Utils
{
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
        
        public class HashWithChecksumConverter : JsonConverter<string>
        {
            public override string Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var hex = reader.GetString();
                CEP57Checksum.Decode(hex, out var checksumResult);
                if (checksumResult == CEP57Checksum.InvalidChecksum)
                    throw new JsonException("Wrong checksum in hexadecimal string.");

                return hex;
            }

            public override void Write(
                Utf8JsonWriter writer,
                string hash,
                JsonSerializerOptions options) =>
                writer.WriteStringValue(hash);
        }
    }
}
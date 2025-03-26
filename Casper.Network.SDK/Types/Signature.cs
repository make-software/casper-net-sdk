using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A wrapper for a cryptographic signature. 
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// Byte array without the Key algorithm identifier.
        /// </summary>
        public byte[] RawBytes { get; }
        
        public KeyAlgo KeyAlgorithm { get;  }

        protected Signature(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            RawBytes = rawBytes;
            KeyAlgorithm = keyAlgo;
        }

        /// <summary>
        /// Creates a Signature object from an hexadecimal string (containing the
        /// Key algorithm identifier).
        /// </summary>
        public static Signature FromHexString(string signature)
        {
            var rawBytes = Hex.Decode(signature.Substring(2));

            KeyAlgo algo = signature.Substring(0, 2) switch
            {
                "01" => KeyAlgo.ED25519,
                "02" => KeyAlgo.SECP256K1,
                _ => throw new ArgumentException("Wrong public key algorithm identifier")
            };
            
            return FromRawBytes(rawBytes, algo);
        }
        
        /// <summary>
        /// Creates a PublicKey object from a byte array (containing the
        /// Key algorithm identifier).
        /// </summary>
        public static Signature FromBytes(byte[] bytes)
        {
            var algoIdent = bytes[0] switch
            {
                0x01 => KeyAlgo.ED25519,
                0x02 => KeyAlgo.SECP256K1,
                _ => throw new ArgumentOutOfRangeException(nameof(bytes), "Wrong signature algorithm identifier")
            };

            return new Signature(bytes.Slice(1), algoIdent);
        }

        /// <summary>
        /// Creates a Signature object from a byte array and the key algorithm identifier.
        /// </summary>
        public static Signature FromRawBytes(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            return new Signature(rawBytes, keyAlgo);
        }

        /// <summary>
        /// Returns the bytes of the signature, including the Key algorithm as the first byte.
        /// </summary>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[1 + RawBytes.Length];
            bytes[0] = KeyAlgorithm switch
            {
                KeyAlgo.ED25519 => 0x01,
                KeyAlgo.SECP256K1 => 0x02,
                _ => 0x00
            };
            Array.Copy(RawBytes, 0, bytes, 1, RawBytes.Length);

            return bytes;
        }

        /// <summary>
        /// Returns the signature as an hexadecimal string, including the key algorithm in the first position.
        /// </summary>
        public string ToHexString()
        {
            if (KeyAlgorithm == KeyAlgo.ED25519)
                return "01" + Hex.ToHexString(RawBytes);
            else
                return "02" + Hex.ToHexString(RawBytes);
        }
        
        public override string ToString()
        {
            return ToHexString();
        }
        
        public class SignatureConverter : JsonConverter<Signature>
        {
            public override Signature Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                {
                    try
                    {
                        return Signature.FromHexString(reader.GetString());
                    }
                    catch (Exception e)
                    {
                        throw new JsonException(e.Message);
                    }
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                Signature signature,
                JsonSerializerOptions options) =>
                writer.WriteStringValue(signature.ToHexString());
        }
    }
}
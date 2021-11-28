using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class Signature
    {
        public byte[] RawBytes { get; }
        public KeyAlgo KeyAlgorithm { get;  }

        protected Signature(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            RawBytes = rawBytes;
            KeyAlgorithm = keyAlgo;
        }

        public static Signature FromHexString(string signature)
        {
            return FromBytes(Hex.Decode(signature));
        }
        
        public static Signature FromBytes(byte[] bytes)
        {
            var algoIdent = bytes[0] switch
            {
                0x01 => KeyAlgo.ED25519,
                0x02 => KeyAlgo.SECP256K1,
                _ => throw new ArgumentOutOfRangeException(nameof(bytes), "Wrong signature algorithm identifier")
            };

            return new Signature(bytes[1..], algoIdent);
        }

        public static Signature FromRawBytes(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            return new Signature(rawBytes, keyAlgo);
        }

        public byte[] GetBytes()
        {
            return RawBytes;
        }

        public string ToHexString()
        {
            if (KeyAlgorithm == KeyAlgo.ED25519)
                return "01" + CEP57Checksum.Encode(RawBytes);
            else
                return "02" + CEP57Checksum.Encode(RawBytes);
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
                JsonSerializerOptions options) =>
                Signature.FromHexString(reader.GetString());

            public override void Write(
                Utf8JsonWriter writer,
                Signature signature,
                JsonSerializerOptions options) =>
                writer.WriteStringValue(signature.ToHexString());
        }
    }
}
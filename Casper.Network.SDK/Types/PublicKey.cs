using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A wrapper for a Public Key. Provides signature verification functionality.
    /// </summary>
    public class PublicKey
    {
        /// <summary>
        /// Byte array without the Key algorithm identifier.
        /// </summary>
        public byte[] RawBytes { get; }
        
        public KeyAlgo KeyAlgorithm { get; }

        protected PublicKey(byte[] rawBytes, KeyAlgo keyAlgorithm)
        {
            RawBytes = rawBytes;
            KeyAlgorithm = keyAlgorithm;
        }

        /// <summary>
        /// Creates a PublicKey object from an hexadecimal string (containing the
        /// Key algorithm identifier).
        /// </summary>
        public static PublicKey FromHexString(string hexBytes)
        {
            var rawBytes = CEP57Checksum.Decode(hexBytes.Substring(2), out var checksumResult);

            if (checksumResult == CEP57Checksum.InvalidChecksum)
                throw new ArgumentException("Public key checksum mismatch.");

            KeyAlgo algo = hexBytes.Substring(0, 2) switch
            {
                "01" => KeyAlgo.ED25519,
                "02" => KeyAlgo.SECP256K1,
                _ => throw new ArgumentException("Wrong public key algorithm identifier")
            };
            return FromRawBytes(rawBytes, algo);
        }

        /// <summary>
        /// Loads a PublicKey from a PEM file 
        /// </summary>
        public static PublicKey FromPem(string filePath)
        {
            using (TextReader textReader = new StringReader(File.ReadAllText(filePath)))
            {
                var reader = new PemReader(textReader);

                var pemObject = reader.ReadObject();

                if (pemObject is Ed25519PublicKeyParameters edPk)
                {
                    byte[] rawBytes = edPk.GetEncoded();
                    return new PublicKey(rawBytes, KeyAlgo.ED25519);
                }

                if (pemObject is ECPublicKeyParameters ecPk)
                {
                    byte[] compressed = ecPk.Q.GetEncoded(true);
                    return new PublicKey(compressed, KeyAlgo.SECP256K1);
                }

                throw new ArgumentException("Unsupported key format or it's not a private key PEM object.",
                    nameof(filePath));
            }
        }

        /// <summary>
        /// Creates a PublicKey object from a byte array. First byte in the array must contain the
        /// key algorithm identifier.
        /// </summary>
        public static PublicKey FromBytes(byte[] bytes)
        {
            if (bytes.Length < 1)
                throw new ArgumentException("Wrong public key format", nameof(bytes));

            var algoIdent = bytes[0];

            (int expectedPublicKeySize, string algo) = algoIdent switch
            {
                0x01 => (KeyAlgo.ED25519.GetKeySizeInBytes(), Enum.GetName(KeyAlgo.ED25519)),
                0x02 => (KeyAlgo.SECP256K1.GetKeySizeInBytes(), Enum.GetName(KeyAlgo.SECP256K1)),
                _ => throw new ArgumentException("Wrong public key algorithm identifier", nameof(bytes))
            };

            if (bytes.Length < expectedPublicKeySize)
                throw new ArgumentException($"Wrong public key format. Expected length is {expectedPublicKeySize}",
                    nameof(bytes));

            var rawBytes = new byte[expectedPublicKeySize - 1];

            Array.Copy(bytes, 1, rawBytes, 0, expectedPublicKeySize - 1);

            return new PublicKey(rawBytes, algoIdent == 0x01 ? KeyAlgo.ED25519 : KeyAlgo.SECP256K1);
        }

        /// <summary>
        /// Creates a PublicKey object from a byte array and the key algorithm identifier. 
        /// </summary>
        public static PublicKey FromRawBytes(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            int expectedPublicKeySize = keyAlgo switch
            {
                KeyAlgo.ED25519 => KeyAlgo.ED25519.GetKeySizeInBytes() - 1, // -1 because we have rawBytes here
                KeyAlgo.SECP256K1 => KeyAlgo.SECP256K1.GetKeySizeInBytes() - 1,
                _ => throw new ArgumentException("Wrong public key algorithm identifier", nameof(keyAlgo))
            };

            if (rawBytes.Length != expectedPublicKeySize)
                throw new ArgumentException($"Wrong public key format. Expected length is {expectedPublicKeySize}",
                    nameof(rawBytes));

            return new PublicKey(rawBytes, keyAlgo);
        }

        /// <summary>
        /// Saves the public key to a PEM file.
        /// </summary>
        public void WriteToPem(string filePath)
        {
            if (File.Exists(filePath))
                throw new Exception("Target file already exists. Will not overwrite." +
                                    Environment.NewLine + "File: " + filePath);
            
            using (var textWriter = File.CreateText(filePath))
            {
                var writer = new PemWriter(textWriter);

                if (KeyAlgorithm == KeyAlgo.ED25519)
                {
                    Ed25519PublicKeyParameters pk = new Ed25519PublicKeyParameters(RawBytes, 0);
                    writer.WriteObject(pk);
                    return;
                }
                else if (KeyAlgorithm == KeyAlgo.SECP256K1)
                {
                    var curve = ECNamedCurveTable.GetByName("secp256k1");
                    var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                    ECPoint q = curve.Curve.DecodePoint(RawBytes);
                    ECPublicKeyParameters pk = new ECPublicKeyParameters(q, domainParams);
                    writer.WriteObject(pk);
                    return;
                }

                throw new Exception("Unsupported key type.");
            }
        }

        /// <summary>
        /// Returns the Account Hash associated to this Public Key.
        /// </summary>
        public string GetAccountHash()
        {
            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            string algo = KeyAlgorithm.ToString().ToLowerInvariant();
            bcBl2bdigest.BlockUpdate(System.Text.Encoding.UTF8.GetBytes(algo), 0, algo.Length);
            bcBl2bdigest.Update(0x00);
            bcBl2bdigest.BlockUpdate(RawBytes, 0, RawBytes.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return "account-hash-" + CEP57Checksum.Encode(hash);
        }

        /// <summary>
        /// Returns the key as an hexadecimal string, including the key algorithm in the first position.
        /// </summary>
        public string ToAccountHex()
        {
            return KeyAlgorithm switch
            {
                KeyAlgo.ED25519 => "01",
                KeyAlgo.SECP256K1 => "02",
                _ => throw new Exception("Wrong key type")
            } + CEP57Checksum.Encode(RawBytes);
        }

        public override string ToString()
        {
            return ToAccountHex();
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || !GetType().Equals(obj.GetType()))
                return false;

            var pk = (PublicKey) obj;
            return pk.GetBytes().SequenceEqual(this.GetBytes());
        }

        public override int GetHashCode()
        {
            return this.ToAccountHex().ToLower().GetHashCode();
        }

        /// <summary>
        /// Returns the bytes of the public key, including the Key algorithm as the first byte.
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
        /// Verifies the signature given its value and the original message.
        /// </summary>
        public bool VerifySignature(byte[] message, byte[] signature)
        {
            if (KeyAlgorithm == KeyAlgo.ED25519)
            {
                Ed25519PublicKeyParameters edPk = new Ed25519PublicKeyParameters(RawBytes, 0);
                return Ed25519.Verify(signature, 0, RawBytes, 0, message, 0, message.Length);
            }

            if (KeyAlgorithm == KeyAlgo.SECP256K1)
            {
                var curve = ECNamedCurveTable.GetByName("secp256k1");
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                ECPoint q = curve.Curve.DecodePoint(RawBytes);
                ECPublicKeyParameters pk = new ECPublicKeyParameters(q, domainParams);

                var signer = SignerUtilities.GetSigner("SHA-256withPLAIN-ECDSA");
                signer.Init(forSigning: false, pk);
                signer.BlockUpdate(message, 0, message.Length);
                return signer.VerifySignature(signature);
            }

            throw new Exception("Unsupported key type.");
        }

        /// <summary>
        /// Verifies the signature given its value and the original message.
        /// </summary>
        public bool VerifySignature(string message, string signature)
        {
            return VerifySignature(Hex.Decode(message), Hex.Decode(signature));
        }

        #region Cast operators

        public static explicit operator string(PublicKey pk) => pk.ToAccountHex();

        #endregion

        /// <summary>
        /// Json converter class to serialize/deserialize a PublicKey to/from Json
        /// </summary>
        public class PublicKeyConverter : JsonConverter<PublicKey>
        {
            public override PublicKey Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    return PublicKey.FromHexString(reader.GetString());
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                PublicKey publicKey,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(publicKey.ToAccountHex());
            }
        }
    }
}
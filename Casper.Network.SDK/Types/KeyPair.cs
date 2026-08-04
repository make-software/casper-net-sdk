using System;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using PemWriter = Org.BouncyCastle.OpenSsl.PemWriter;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A Private key. Provides signing functionality.
    /// </summary>
    public class KeyPair
    {
        private const int PrivateKeyByteSize = 32;

        /// <summary>
        /// The public key derived from the private key.
        /// </summary>
        public PublicKey PublicKey { get; private set; }

        /// <summary>
        /// The raw bytes of the private key.
        /// For ED25519, the 32-byte seed.
        /// For SECP256K1, the 32-byte big-endian unsigned scalar.
        /// </summary>
        public byte[] RawBytes => PrivateKeyRawBytes();

        private AsymmetricKeyParameter _privateKey;

        /// <summary>
        /// Loads a key pair from a PEM file containing a private key.
        /// </summary>
        public static KeyPair FromPem(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"PEM file not found: {filePath}", filePath);

            using TextReader textReader = File.OpenText(filePath);
            using var reader = new PemReader(textReader);
            var pemObject = reader.ReadObject();
            switch (pemObject)
            {
                case Ed25519PrivateKeyParameters privateKey:
                {
                    var publicKey = privateKey.GeneratePublicKey();

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(publicKey.GetEncoded(), KeyAlgo.ED25519),
                        _privateKey = privateKey
                    };
                }
                case AsymmetricCipherKeyPair keyPair:
                {
                    var privKey = (ECPrivateKeyParameters) keyPair.Private;
                    if (privKey.PublicKeyParamSet.Id != "1.3.132.0.10")
                        throw new Exception(
                            $"Wrong curve type. OID expected 1.3.132.0.10. Found {privKey.PublicKeyParamSet.Id}");

                    var q = privKey.Parameters.G.Multiply(privKey.D);
                    var pub = new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);

                    var compressed = pub.Q.GetEncoded(true);

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(compressed, KeyAlgo.SECP256K1),
                        _privateKey = keyPair.Private
                    };
                }
                default:
                    throw new ArgumentException("Unsupported key format or it's not a private key PEM object.",
                        nameof(filePath));
            }
        }

        /// <summary>
        /// Loads a key pair from a raw byte array containing the private key.
        /// For ED25519, the 32-byte seed is expected.
        /// For SECP256K1, the 32-byte big-endian unsigned scalar is expected.
        /// </summary>
        public static KeyPair FromBytes(byte[] bytes, KeyAlgo keyAlgorithm)
        {
            if (bytes.Length != PrivateKeyByteSize)
                throw new ArgumentException(
                    $"Wrong private key format. Expected length is {PrivateKeyByteSize}",
                    nameof(bytes));

            switch (keyAlgorithm)
            {
                case KeyAlgo.ED25519:
                {
                    var privKey = new Ed25519PrivateKeyParameters(bytes, 0);
                    var publicKey = privKey.GeneratePublicKey();

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(publicKey.GetEncoded(), KeyAlgo.ED25519),
                        _privateKey = privKey
                    };
                }
                case KeyAlgo.SECP256K1:
                {
                    var curve = ECNamedCurveTable.GetByName("secp256k1");
                    var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                    var d = new BigInteger(1, bytes);
                    var privKey = new ECPrivateKeyParameters("ECDSA", d, domainParams);
                    var q = privKey.Parameters.G.Multiply(privKey.D);

                    var pub = new ECPublicKeyParameters(q, domainParams);
                    var compressed = pub.Q.GetEncoded(true);

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(compressed, KeyAlgo.SECP256K1),
                        _privateKey = privKey
                    };
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyAlgorithm), keyAlgorithm,
                        $"Unsupported key algorithm: {keyAlgorithm}");
            }
        }

        [Obsolete("Use Create() instead.", false)]
        public static KeyPair CreateNew(KeyAlgo keyAlgorithm)
        {
            return Create(keyAlgorithm);
        }

        /// <summary>
        /// Creates a new key pair with the specified algorithm.
        /// </summary>
        public static KeyPair Create(KeyAlgo keyAlgorithm)
        {
            if (keyAlgorithm == KeyAlgo.ED25519)
            {
                var gen = new Ed25519KeyPairGenerator();
                gen.Init(new KeyGenerationParameters(new SecureRandom(), 255));
                var newKey = gen.GenerateKeyPair();
                var publicKey = (Ed25519PublicKeyParameters) newKey.Public;

                return new KeyPair()
                {
                    PublicKey = PublicKey.FromRawBytes(publicKey.GetEncoded(), KeyAlgo.ED25519),
                    _privateKey = newKey.Private
                };
            }
            else
            {
                var curve = ECNamedCurveTable.GetByName("secp256k1");
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                var secureRandom = new SecureRandom();
                var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

                var generator = new ECKeyPairGenerator("ECDSA");
                generator.Init(keyParams);
                var newKey = generator.GenerateKeyPair();

                var pk = newKey.Public as ECPublicKeyParameters;
                var compressed = pk.Q.GetEncoded(true);

                return new KeyPair()
                {
                    PublicKey = PublicKey.FromRawBytes(compressed, KeyAlgo.SECP256K1),
                    _privateKey = newKey.Private
                };
            }
        }

        /// <summary>
        /// Saves the private key to a PEM file.
        /// </summary>
        public void WriteToPem(string filePath)
        {
            if (File.Exists(filePath))
                throw new Exception("Target file already exists. Will not overwrite." +
                    Environment.NewLine + "File: " + filePath);
            
            if (PublicKey.KeyAlgorithm == KeyAlgo.ED25519)
            {
                using var textWriter = File.CreateText(filePath);
                var writer = new PemWriter(textWriter);
                writer.WriteObject(_privateKey);
            }
            else
            {
                var bytes =
                    Hex.Decode(
                        "302E02010104200000000000000000000000000000000000000000000000000000000000000000A00706052B8104000A");
                var privKey = (ECPrivateKeyParameters) _privateKey;
                var skbytes = privKey.D.ToByteArrayUnsigned();
                Array.Copy(skbytes, 0, bytes, 7 + (32 - skbytes.Length), skbytes.Length);
                using var textWriter = File.CreateText(filePath);
                var writer = new PemWriter(textWriter);
                writer.WriteObject(new PemObject("EC PRIVATE KEY", bytes));
            }
        }

        /// <summary>
        /// Saves the public key to a PEM file.
        /// </summary>
        public void WritePublicKeyToPem(string filePath)
        {
            PublicKey.WriteToPem(filePath);
        }

        /// <summary>
        /// Signs a message and returns the signature.
        /// </summary>
        public byte[] Sign(byte[] message)
        {
            switch (PublicKey.KeyAlgorithm)
            {
                case KeyAlgo.ED25519:
                {
                    var signature = new byte[Ed25519.SignatureSize];
                    var sk = (Ed25519PrivateKeyParameters) _privateKey;
                    Ed25519.Sign(sk.GetEncoded(), 0, message, 0, message.Length, signature, 0);

                    return signature;
                }
                case KeyAlgo.SECP256K1:
                {
                    var k = new SecureRandom();
                    var param = new ParametersWithRandom(_privateKey, k);

                    var signer = SignerUtilities.GetSigner("SHA-256withPLAIN-ECDSA");
                    signer.Init(forSigning: true, param);
                    signer.BlockUpdate(message, 0, message.Length);
                    var rs = signer.GenerateSignature();

                    if ((rs[32] & 0x80) == 0x80)
                    {
                        var curve = ECNamedCurveTable.GetByName("secp256k1");
                        var n = curve.N;
                        var sBytes = new byte[32];
                        Array.Copy(rs, 32, sBytes, 0, 32);
                        var lowS = n.Subtract(new BigInteger(1, sBytes)).ToByteArrayUnsigned();
                        Array.Clear(rs, 32, 32);
                        Array.Copy(lowS, 0, rs, 32 + (32 - lowS.Length), lowS.Length);
                    }

                    return rs;
                }
                default:
                    throw new Exception("Unsupported key type.");
            }
        }

        private byte[] PrivateKeyRawBytes()
        {
            if (PublicKey.KeyAlgorithm == KeyAlgo.ED25519)
            {
                var privKey = (Ed25519PrivateKeyParameters) _privateKey;
                return privKey.GetEncoded();
            }
            else
            {
                var privKey = (ECPrivateKeyParameters) _privateKey;
                var bytes = new byte[PrivateKeyByteSize];
                var skbytes = privKey.D.ToByteArrayUnsigned();
                Array.Copy(skbytes, 0, bytes, PrivateKeyByteSize - skbytes.Length, skbytes.Length);
                return bytes;
            }
        }
    }
}

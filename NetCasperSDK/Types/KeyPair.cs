using System;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using PemWriter = Org.BouncyCastle.OpenSsl.PemWriter;

namespace NetCasperSDK.Types
{
    public class KeyPair
    {
        public PublicKey PublicKey { get; private set; }

        private AsymmetricKeyParameter _publicKey;
        private AsymmetricKeyParameter _privateKey;

        public static KeyPair FromPem(string filePath)
        {
            using (TextReader textReader = new StringReader(File.ReadAllText(filePath)))
            {
                var reader = new PemReader(textReader);

                var pemObject = reader.ReadObject();
                if (pemObject is Ed25519PrivateKeyParameters privateKey)
                {
                    var publicKey = privateKey.GeneratePublicKey();

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(publicKey.GetEncoded(), KeyAlgo.ED25519),
                        _publicKey = publicKey,
                        _privateKey = privateKey
                    };
                }

                if (pemObject is AsymmetricCipherKeyPair keyPair)
                {
                    var privKey = (ECPrivateKeyParameters) keyPair.Private;
                    if (privKey.PublicKeyParamSet.Id != "1.3.132.0.10")
                        throw new Exception(
                            $"Wrong curve type. OID expected 1.3.132.0.10. Found ${privKey.PublicKeyParamSet.Id}");

                    var q = privKey.Parameters.G.Multiply(privKey.D);
                    var pub = new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);

                    byte[] compressed = pub.Q.GetEncoded(true);

                    return new KeyPair()
                    {
                        PublicKey = PublicKey.FromRawBytes(compressed, KeyAlgo.SECP256K1),
                        _publicKey = keyPair.Public,
                        _privateKey = keyPair.Private
                    };
                }

                throw new ArgumentException("Unsupported key format or it's not a private key PEM object.",
                    nameof(filePath));
            }
        }

        public static KeyPair CreateNew(KeyAlgo keyAlgorithm)
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
                    _publicKey = publicKey,
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
                byte[] compressed = pk.Q.GetEncoded(true);

                return new KeyPair()
                {
                    PublicKey = PublicKey.FromRawBytes(compressed, KeyAlgo.SECP256K1),
                    _publicKey = newKey.Public,
                    _privateKey = newKey.Private
                };
            }
        }

        public void WriteToPem(string filePath)
        {
            if (PublicKey.KeyAlgorithm == KeyAlgo.ED25519)
            {
                using (var textWriter = File.CreateText(filePath))
                {
                    var writer = new PemWriter(textWriter);
                    writer.WriteObject(_privateKey);
                }
            }
            else
            {
                byte[] bytes =
                    Hex.Decode(
                        "302E02010104200000000000000000000000000000000000000000000000000000000000000000A00706052B8104000A");
                var privKey = (ECPrivateKeyParameters) _privateKey;
                var skbytes = privKey.D.ToByteArrayUnsigned();
                Array.Copy(skbytes, 0, bytes, 7 + (32 - skbytes.Length), skbytes.Length);
                using (var textWriter = File.CreateText(filePath))
                {
                    var writer = new PemWriter(textWriter);
                    writer.WriteObject(new PemObject("EC PRIVATE KEY", bytes));
                }
            }
        }

        public void WritePublicKeyToPem(string filePath)
        {
            using (var textWriter = File.CreateText(filePath))
            {
                var writer = new PemWriter(textWriter);
                writer.WriteObject(_publicKey);
            }
        }

        public byte[] Sign(byte[] message)
        {
            if (PublicKey.KeyAlgorithm == KeyAlgo.ED25519)
            {
                byte[] signature = new byte[Ed25519.SignatureSize];
                var sk = (Ed25519PrivateKeyParameters) _privateKey;
                Ed25519.Sign(sk.GetEncoded(), 0, message, 0, message.Length, signature, 0);

                return signature;
            }

            if (PublicKey.KeyAlgorithm == KeyAlgo.SECP256K1)
            {
                SecureRandom k = new SecureRandom();
                ParametersWithRandom param = new ParametersWithRandom(_privateKey, k);

                var signer = SignerUtilities.GetSigner("SHA-256withPLAIN-ECDSA");
                signer.Init(forSigning: true, param);
                signer.BlockUpdate(message, 0, message.Length);
                byte[] rs = signer.GenerateSignature();

                return rs;
            }

            throw new Exception("Unsupported key type.");
        }
    }
}
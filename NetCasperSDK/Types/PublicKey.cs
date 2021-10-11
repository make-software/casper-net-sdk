using System;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperSDK.Types
{
    public class PublicKey
    {
        public byte[] RawBytes { get; }
        public KeyAlgo KeyAlgorithm { get; }

        protected PublicKey(byte[] rawBytes, KeyAlgo keyAlgorithm)
        {
            RawBytes = rawBytes;
            KeyAlgorithm = keyAlgorithm;
        }

        public static PublicKey FromHexString(string hexBytes)
        {
            return FromBytes(Hex.Decode(hexBytes));
        }

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

        public static PublicKey FromRawBytes(byte[] rawBytes, KeyAlgo keyAlgo)
        {
            return new PublicKey(rawBytes, keyAlgo);
        }

        public void WriteToPem(string filePath)
        {
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

        public class ECPublicKeySpec
        {
            public ECPublicKeySpec(object o, object o1)
            {
                throw new NotImplementedException();
            }
        }

        public byte[] GetAccountHash()
        {
            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            string algo = KeyAlgorithm.ToString().ToLower();
            bcBl2bdigest.BlockUpdate(System.Text.Encoding.UTF8.GetBytes(algo), 0, algo.Length);
            bcBl2bdigest.Update(0x00);
            bcBl2bdigest.BlockUpdate(RawBytes, 0, RawBytes.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }

        public String ToAccountHex()
        {
            return KeyAlgorithm switch
            {
                KeyAlgo.ED25519 => "01",
                KeyAlgo.SECP256K1 => "02",
                _ => throw new Exception("Wrong key type")
            } + Hex.ToHexString(RawBytes);
        }

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
    }
}
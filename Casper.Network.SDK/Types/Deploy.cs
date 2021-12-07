using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class Deploy
    {
        [JsonPropertyName("approvals")] 
        public List<DeployApproval> Approvals { get; } = new List<DeployApproval>();

        /// <summary>
        /// A hash over the header of the deploy.
        /// </summary>
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(CEP57Checksum.HashWithChecksumConverter))]
        public string Hash { get; }

        /// <summary>
        /// Contains metadata about the deploy.
        /// </summary>
        [JsonPropertyName("header")]
        public DeployHeader Header { get; }

        [JsonPropertyName("payment")]
        [JsonConverter(typeof(ExecutableDeployItemConverter))]
        public ExecutableDeployItem Payment { get; }

        [JsonPropertyName("session")]
        [JsonConverter(typeof(ExecutableDeployItemConverter))]
        public ExecutableDeployItem Session { get; }

        public static Deploy Load(string filename)
        {
            var data = File.ReadAllText(filename);
            return Deploy.Parse(data);
        }

        public static Deploy Parse(string json)
        {
            try
            {
                var deploy = JsonSerializer.Deserialize<Deploy>(json);

                return deploy;
            }
            catch (JsonException e)
            {
                var message = $"The JSON value could not be converted to a Deploy object. " +
                              $"{e.Message} Path: {e.Path} | LineNumber: {e.LineNumber} | " +
                              $"BytePositionInLine: {e.BytePositionInLine}.";
                throw new Exception(message);
            }
        }

        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(this));
        }

        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        [JsonConstructor]
        public Deploy(string hash, DeployHeader header, ExecutableDeployItem payment,
            ExecutableDeployItem session, List<DeployApproval> approvals)
        {
            this.Hash = hash;
            this.Header = header;
            this.Payment = payment;
            this.Session = session;
            this.Approvals = approvals;
        }

        public Deploy(DeployHeader header,
            ExecutableDeployItem payment,
            ExecutableDeployItem session)
        {
            var bodyHash = ComputeBodyHash(payment, session);
            this.Header = new DeployHeader()
            {
                Account = header.Account,
                Timestamp = header.Timestamp,
                Ttl = header.Ttl,
                Dependencies = header.Dependencies,
                GasPrice = header.GasPrice,
                BodyHash = CEP57Checksum.Encode(bodyHash),
                ChainName = header.ChainName
            };
            this.Hash = CEP57Checksum.Encode(ComputeHeaderHash(this.Header));
            this.Payment = payment;
            this.Session = session;
        }

        public void Sign(KeyPair keyPair)
        {
            byte[] signature = keyPair.Sign(Hex.Decode(this.Hash));

            Approvals.Add(new DeployApproval()
            {
                Signature = Signature.FromRawBytes(signature, keyPair.PublicKey.KeyAlgorithm),
                Signer = keyPair.PublicKey
            });
        }

        public void AddApproval(DeployApproval approval)
        {
            this.Approvals.Add(approval);
        }

        public bool ValidateHashes(out string message)
        {
            var computedHash = ComputeBodyHash(this.Payment, this.Session);
            if (!Hex.Decode(this.Header.BodyHash).SequenceEqual(computedHash))
            {
                message = "Computed Body Hash does not match value in deploy header. " +
                          $"Expected: '{this.Header.BodyHash}'. " +
                          $"Computed: '{CEP57Checksum.Encode(computedHash)}'.";
                return false;
            }

            computedHash = ComputeHeaderHash(this.Header);
            if (!Hex.Decode(this.Hash).SequenceEqual(computedHash))
            {
                message = "Computed Hash does not match value in deploy object. " +
                          $"Expected: '{this.Hash}'. " +
                          $"Computed: '{CEP57Checksum.Encode(computedHash)}'.";
                return false;
            }

            message = "";
            return true;
        }

        public bool VerifySignatures(out string message)
        {
            message = string.Empty;

            foreach (var approval in Approvals)
            {
                if (!approval.Signer.VerifySignature(Hex.Decode(this.Hash), 
                        approval.Signature.RawBytes))
                {
                    message = $"Error verifying signature with signer '{approval.Signer}'.";
                    return false;
                }
            }

            return true;
        }

        public int GetDeploySizeInBytes()
        {
            var serializer = new DeployByteSerializer();
            return serializer.ToBytes(this).Length;
        }

        private byte[] ComputeBodyHash(ExecutableDeployItem payment, ExecutableDeployItem session)
        {
            var ms = new MemoryStream();
            var itemSerializer = new ExecutableDeployItemByteSerializer();

            ms.Write(itemSerializer.ToBytes(payment));
            ms.Write(itemSerializer.ToBytes(session));

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            var bBody = ms.ToArray();

            bcBl2bdigest.BlockUpdate(bBody, 0, bBody.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }

        private byte[] ComputeHeaderHash(DeployHeader header)
        {
            var serializer = new DeployByteSerializer();
            var bHeader = serializer.ToBytes(header);

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);

            bcBl2bdigest.BlockUpdate(bHeader, 0, bHeader.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }
    }
}
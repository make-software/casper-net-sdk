using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class Deploy
    {
        /// <summary>
        /// List of signers and signatures for this Deploy.
        /// </summary>
        [JsonPropertyName("approvals")] 
        public List<Approval> Approvals { get; } = new List<Approval>();

        /// <summary>
        /// A hash over the header of the deploy.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; }

        /// <summary>
        /// Contains metadata about the deploy.
        /// </summary>
        [JsonPropertyName("header")]
        public DeployHeader Header { get; }

        /// <summary>
        /// Contains the payment amount for the deploy.
        /// </summary>
        [JsonPropertyName("payment")]
        [JsonConverter(typeof(ExecutableDeployItemConverter))]
        public ExecutableDeployItem Payment { get; }

        /// <summary>
        /// Contains the session information for the deploy.
        /// </summary>
        [JsonPropertyName("session")]
        [JsonConverter(typeof(ExecutableDeployItemConverter))]
        public ExecutableDeployItem Session { get; }

        /// <summary>
        /// Loads and deserializes a Deploy from a file.
        /// </summary>
        public static Deploy Load(string filename)
        {
            var data = File.ReadAllText(filename);
            return Deploy.Parse(data);
        }

        /// <summary>
        /// Parses a Deploy from a file.
        /// </summary>
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

        /// <summary>
        /// Saves a deploy object to a file.
        /// </summary>
        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(this));
        }

        /// <summary>
        /// Returns a json string with the deploy.
        /// </summary>
        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        [JsonConstructor]
        public Deploy(string hash, DeployHeader header, ExecutableDeployItem payment,
            ExecutableDeployItem session, List<Approval> approvals)
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
                BodyHash = Hex.ToHexString(bodyHash),
                ChainName = header.ChainName
            };
            this.Hash = Hex.ToHexString(ComputeHeaderHash(this.Header));
            this.Payment = payment;
            this.Session = session;
        }

        /// <summary>
        /// Signs the deploy with a private key and adds a new Approval to it.
        /// </summary>
        public void Sign(KeyPair keyPair)
        {
            byte[] signature = keyPair.Sign(Hex.Decode(this.Hash));

            Approvals.Add(new Approval()
            {
                Signature = Signature.FromRawBytes(signature, keyPair.PublicKey.KeyAlgorithm),
                Signer = keyPair.PublicKey
            });
        }

        /// <summary>
        /// Adds an approval to the deploy. No check is done to the approval signature.
        /// </summary>
        public void AddApproval(Approval approval)
        {
            this.Approvals.Add(approval);
        }

        /// <summary>
        /// Validates the body and header hashes in the deploy.
        /// </summary>
        /// <param name="message">output string with a validation error message if validation fails. empty otherwise.</param>
        /// <returns>false if the validation of hashes is not successful</returns>
        public bool ValidateHashes(out string message)
        {
            var computedHash = ComputeBodyHash(this.Payment, this.Session);
            if (!Hex.Decode(this.Header.BodyHash).SequenceEqual(computedHash))
            {
                message = "Computed Body Hash does not match value in deploy header. " +
                          $"Expected: '{this.Header.BodyHash}'. " +
                          $"Computed: '{Hex.ToHexString(computedHash)}'.";
                return false;
            }

            computedHash = ComputeHeaderHash(this.Header);
            if (!Hex.Decode(this.Hash).SequenceEqual(computedHash))
            {
                message = "Computed Hash does not match value in deploy object. " +
                          $"Expected: '{this.Hash}'. " +
                          $"Computed: '{Hex.ToHexString(computedHash)}'.";
                return false;
            }

            message = "";
            return true;
        }

        /// <summary>
        /// Verifies the signatures in the list of approvals.
        /// </summary>
        /// <param name="message">an output string with the signer which signature could not be verified. empty if verification succeeds.</param>
        /// <returns>false if the verification of a signature fails.</returns>
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

        /// <summary>
        /// returns the number of bytes resulting from the binary serialization of the Deploy.
        /// </summary>
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A unit of work sent by a client to the network, which when executed can cause global state to be altered.
    /// </summary>
    public class TransactionV1
    {
        /// <summary>
        /// A hash over the header of the transaction.
        /// </summary>
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(CEP57Checksum.HashWithChecksumConverter))]
        public string Hash { get; }
        
        /// <summary>
        /// List of signers and signatures for this transaction.
        /// </summary>
        [JsonPropertyName("approvals")] 
        public List<Approval> Approvals { get; } = new List<Approval>();
        
        /// <summary>
        /// Header for this transaction.
        /// </summary>
        [JsonPropertyName("header")] 
        public TransactionV1Header Header { get; init; }

        /// <summary>
        /// Body for this transaction.
        /// </summary>
        [JsonPropertyName("body")] 
        public TransactionV1Body Body { get; init; }
        
        /// <summary>
        /// Loads and deserializes a TransactionV1 from a file.
        /// </summary>
        public static TransactionV1 Load(string filename)
        {
            var data = File.ReadAllText(filename);
            return TransactionV1.Parse(data);
        }
        
        /// <summary>
        /// Parses a Transaction from a string with json.
        /// </summary>
        public static TransactionV1 Parse(string json)
        {
            try
            {
                var transaction = JsonSerializer.Deserialize<TransactionV1>(json);

                return transaction;
            }
            catch (JsonException e)
            {
                var message = $"The JSON value could not be converted to a TransactionV1 object. " +
                              $"{e.Message} Path: {e.Path} | LineNumber: {e.LineNumber} | " +
                              $"BytePositionInLine: {e.BytePositionInLine}.";
                throw new Exception(message);
            }
        }
        
        /// <summary>
        /// Saves a transaction object to a file.
        /// </summary>
        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(this));
        }
        
        /// <summary>
        /// Returns a json string with the transaction.
        /// </summary>
        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        
        [JsonConstructor]
        public TransactionV1(string hash,
            TransactionV1Header header,
            TransactionV1Body body,
            List<Approval> approvals)
        {
            this.Hash = hash;
            this.Header = header;
            this.Body = body;
            this.Approvals = approvals;
        }
        
        public TransactionV1(TransactionV1Header header,
            TransactionV1Body body)
        {
            var bodyHash = ComputeBodyHash(body);
            this.Header = new TransactionV1Header()
            {
                ChainName = header.ChainName,
                Timestamp = header.Timestamp,
                Ttl = header.Ttl,
                BodyHash = Hex.ToHexString(bodyHash),
                PricingMode = header.PricingMode,
                InitiatorAddr = header.InitiatorAddr,
            };
            this.Hash = Hex.ToHexString(ComputeHeaderHash(this.Header));
            this.Body = body;
        }
        
        /// <summary>
        /// Signs the transaction with a private key and adds a new Approval to it.
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
        /// Adds an approval to the transaction. No check is done to the approval signature.
        /// </summary>
        public void AddApproval(Approval approval)
        {
            this.Approvals.Add(approval);
        }
        
        /// <summary>
        /// Validates the body and header hashes in the transaction.
        /// </summary>
        /// <param name="message">output string with a validation error message if validation fails. empty otherwise.</param>
        /// <returns>false if the validation of hashes is not successful</returns>
        public bool ValidateHashes(out string message)
        {
            var computedHash = ComputeBodyHash(this.Body);
            if (!Hex.Decode(this.Header.BodyHash).SequenceEqual(computedHash))
            {
                message = "Computed Body Hash does not match value in transaction header. " +
                          $"Expected: '{this.Header.BodyHash}'. " +
                          $"Computed: '{computedHash}'.";
                return false;
            }

            computedHash = ComputeHeaderHash(this.Header);
            if (!Hex.Decode(this.Hash).SequenceEqual(computedHash))
            {
                message = "Computed Hash does not match value in transaction object. " +
                          $"Expected: '{this.Hash}'. " +
                          $"Computed: '{computedHash}'.";
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
        public int GetTransactionSizeInBytes()
        {
            var serializer = new TransactionV1ByteSerializer();
            return serializer.ToBytes(this).Length;
        }
        
        private byte[] ComputeBodyHash(TransactionV1Body body)
        {
            var ms = new MemoryStream();

            var serializer = new TransactionV1ByteSerializer();
            
            ms.Write(serializer.ToBytes(body));

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
            var bBody = ms.ToArray();

            bcBl2bdigest.BlockUpdate(bBody, 0, bBody.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }
        
        private byte[] ComputeHeaderHash(TransactionV1Header header)
        {
            var serializer = new TransactionV1ByteSerializer();
            var bHeader = serializer.ToBytes(header);

            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);

            bcBl2bdigest.BlockUpdate(bHeader, 0, bHeader.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }
    }
}
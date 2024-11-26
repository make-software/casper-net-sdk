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
        public string Hash { get; }

        /// <summary>
        /// Payload  for this transaction.
        /// </summary>
        [JsonPropertyName("payload")]
        public TransactionV1Payload Payload { get; init; }

        /// <summary>
        /// List of signers and signatures for this transaction.
        /// </summary>
        [JsonPropertyName("approvals")]
        public List<Approval> Approvals { get; } = new List<Approval>();
        
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
            TransactionV1Payload payload,
            List<Approval> approvals)
        {
            this.Hash = hash;
            this.Payload = payload;
            this.Approvals = approvals;
        }

        public TransactionV1(TransactionV1Payload payload)
        {
            var payloadBytes = payload.ToBytes();
            var blake2BDigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);

            blake2BDigest.BlockUpdate(payloadBytes, 0, payloadBytes.Length);

            var hash = new byte[blake2BDigest.GetDigestSize()];
            blake2BDigest.DoFinal(hash, 0);

            this.Payload = payload;
            this.Hash = Hex.ToHexString(hash);
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
        /// Validates the transaction hash.
        /// </summary>
        /// <param name="message">output string with a validation error message if validation fails. empty otherwise.</param>
        /// <returns>false if the validation of hash is not successful</returns>
        public bool ValidateHashes(out string message)
        {
            var computedHash = this.ToBytes();
            if (!Hex.Decode(this.Hash).SequenceEqual(computedHash))
            {
                message = "Computed hash does not match value in transaction object." +
                          $"Expected: '{this.Hash}'. " +
                          $"Computed: '{computedHash}'.";
                return false;
            }

            message = "";
            return false;
        }

        /// <summary>
        /// Verifies the signatures in the list of approvals.
        /// </summary>
        /// <param name="message">an output string with the signer which signature could not be verified. empty if verification succeeds.</param>
        /// <returns>false if the verification of a signature fails.</returns>
        public bool VerifySignatures(out string message)
        {
            message = string.Empty;
            
            var decodedHash = Hex.Decode(this.Hash);

            foreach (var approval in Approvals.Where(approval => !approval.Signer
                         .VerifySignature(decodedHash, approval.Signature.RawBytes)))
            {
                message = $"Error verifying signature with signer '{approval.Signer}'.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// returns the number of bytes resulting from the binary serialization of the Deploy.
        /// </summary>
        public int GetTransactionSizeInBytes()
        {
            return this.ToBytes().Length;
        }

        const ushort HASH_FIELD_INDEX = 0;
        const ushort PAYLOAD_FIELD_INDEX = 1;
        const ushort APPROVALS_FIELD_INDEX = 2;

            
        public byte[] ToBytes()
        {
            // add the approvals
            //
            var ms = new MemoryStream();
            var count = LittleEndianConverter.GetBytes(this.Approvals.Count);
            ms.Write(count, 0, count.Length);
            foreach (var approval in this.Approvals)
            {
                var approvalSerializer = new DeployApprovalByteSerializer();
                var approvalBytes = approvalSerializer.ToBytes(approval); 
                ms.Write(approvalBytes, 0, approvalBytes.Length);
            }
            
            return new CalltableSerialization()
                .AddField(HASH_FIELD_INDEX, Hex.Decode(this.Hash))
                .AddField(PAYLOAD_FIELD_INDEX, this.Payload.ToBytes())
                .AddField(APPROVALS_FIELD_INDEX, ms.ToArray())
                .GetBytes();
        }
    }
}
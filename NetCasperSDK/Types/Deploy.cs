using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using NetCasperSDK.ByteSerializers;
using NetCasperSDK.Converters;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperSDK.Types
{
    public class Deploy
    {
        
        [JsonPropertyName("approvals")]
        public List<DeployApproval> Approvals { get; } = new List<DeployApproval>();

        /// <summary>
        /// A hash over the header of the deploy.
        /// </summary>
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] Hash { get; }

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
                BodyHash = bodyHash,
                ChainName = header.ChainName
            };
            this.Hash = ComputeHeaderHash(this.Header);
            this.Payment = payment;
            this.Session = session;
        }

        public void Sign(KeyPair keyPair)
        {
            byte[] signature = keyPair.Sign(this.Hash);

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
            if(!this.Header.BodyHash.SequenceEqual(computedHash))
            {
                message = "Computed Body Hash does not match value in deploy header. " +
                          $"Expected: '{Hex.ToHexString(this.Header.BodyHash)}'. " +
                          $"Computed: '{Hex.ToHexString(computedHash)}'.";
                return false;
            }

            computedHash = ComputeHeaderHash(this.Header);
            if (!this.Hash.SequenceEqual(computedHash))
            {
                message = "Computed Hash does not match value in deploy object. " +
                          $"Expected: '{Hex.ToHexString(this.Hash)}'. " +
                          $"Computed: '{Hex.ToHexString(computedHash)}'.";
                return false;
            }
            
            message = "";
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
            
            bcBl2bdigest.BlockUpdate(bBody,0, bBody.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }

        private byte[] ComputeHeaderHash(DeployHeader header)
        {
            var serializer = new DeployByteSerializer();
            var bHeader = serializer.ToBytes(header);
            
            var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);

            bcBl2bdigest.BlockUpdate(bHeader,0, bHeader.Length);

            var hash = new byte[bcBl2bdigest.GetDigestSize()];
            bcBl2bdigest.DoFinal(hash, 0);

            return hash;
        }
    }
}
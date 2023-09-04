using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A block header
    /// </summary>
    public class BlockHeader
    {
        /// <summary>
        /// Accumulated seed.
        /// </summary>
        [JsonPropertyName("accumulated_seed")]
        public string AccumulatedSeed { get; init; }

        /// <summary>
        /// The body hash.
        /// </summary>
        [JsonPropertyName("body_hash")]
        public string BodyHash { get; init; }

        /// <summary>
        /// The era end.
        /// </summary>
        [JsonPropertyName("era_end")]
        public EraEnd EraEnd { get; init; }

        /// <summary>
        /// The block era id.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// The block height.
        /// </summary>
        [JsonPropertyName("height")]
        public ulong Height { get; init; }
        
        /// <summary>
        /// The parent hash.
        /// </summary>
        [JsonPropertyName("parent_hash")]
        public string ParentHash { get; init; }
        
        /// <summary>
        /// The protocol version.
        /// </summary>
        [JsonPropertyName("protocol_version")] 
        public string ProtocolVersion { get; init; }
        
        /// <summary>
        /// Randomness bit.
        /// </summary>
        [JsonPropertyName("random_bit")]
        public bool RandomBit { get; init; }
    
        /// <summary>
        /// The state root hash.
        /// </summary>
        [JsonPropertyName("state_root_hash")]
        public string StateRootHash { get; init; }
        
        /// <summary>
        /// The block timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")] 
        public string Timestamp { get; init; }
    }

    /// <summary>
    /// Validator that proposed the block
    /// </summary>
    public class Proposer
    {
        /// <summary>
        /// The block was proposed by the system, not a validator
        /// </summary>
        public bool isSystem { get; set; }
        
        /// <summary>
        /// Validator's public key
        /// </summary>
        public PublicKey PublicKey { get; set; }
        
        /// <summary>
        /// Json converter class to serialize/deserialize a Proposer to/from Json
        /// </summary>
        public class ProposerConverter : JsonConverter<Proposer>
        {
            public override Proposer Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var pkhex = reader.GetString();
                    if (pkhex == "00")
                        return new Proposer()
                        {
                            isSystem = true,
                        };
                    return new Proposer()
                    {
                        isSystem = false,
                        PublicKey = PublicKey.FromHexString(pkhex),
                    };
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                Proposer proposer,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(proposer.isSystem ? "00" : proposer.PublicKey.ToAccountHex());
            }
        }
    }
    
    /// <summary>
    /// A block body
    /// </summary>
    public class BlockBody
    {
        /// <summary>
        /// List of Deploy hashes included in the block
        /// </summary>
        [JsonPropertyName("deploy_hashes")]
        public List<string> DeployHashes { get; init; }
        
        /// <summary>
        /// Public key of the validator that proposed the block
        /// </summary>
        [JsonPropertyName("proposer")]
        [JsonConverter(typeof(Proposer.ProposerConverter))]
        public Proposer Proposer { get; init; }
        
        /// <summary>
        /// List of Transfer hashes included in the block
        /// </summary>
        [JsonPropertyName("transfer_hashes")]
        public List<string> TransferHashes { get; init; }
    }

    /// <summary>
    /// Block's finality signature.
    /// </summary>
    public class BlockProof
    {
        /// <summary>
        /// Validator public key
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }
        
        /// <summary>
        /// Validator signature
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }
    
    /// <summary>
    /// A block in the network
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBody Body { get; init; }

        /// <summary>
        /// Block hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; init; }

        /// <summary>
        /// Block header
        /// </summary>
        [JsonPropertyName("header")]
        public BlockHeader Header { get; init; }

        /// <summary>
        /// List of proofs for this block.
        /// </summary>
        [JsonPropertyName("proofs")]
        public List<BlockProof> Proofs { get; init; }
    }
}

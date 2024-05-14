using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace Casper.Network.SDK.Types
{
    public abstract class BlockHeader
    {
        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class BlockHeaderConverter : JsonConverter<BlockHeader>
        {
            public override BlockHeader Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    reader.Read();
                    var version = reader.GetString();
                    reader.Read();
                    switch (version.ToLowerInvariant())
                    {
                        case "version1":
                        {
                            var block1 = JsonSerializer.Deserialize<BlockHeaderV1>(ref reader, options);
                            reader.Read();
                            return block1;
                        }
                        case "version2":
                            var block2 = JsonSerializer.Deserialize<BlockHeaderV2>(ref reader, options);
                            reader.Read();
                            return block2;
                        default:
                            throw new JsonException("Expected Version1 or Version2");
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                BlockHeader blockHeader,
                JsonSerializerOptions options)
            {
                if (blockHeader is BlockHeaderV2)
                {
                    writer.WritePropertyName("Version2");
                    writer.WriteStartObject();
                    JsonSerializer.Serialize(blockHeader as BlockHeaderV2, options);
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WritePropertyName("Version1");
                    writer.WriteStartObject();
                    JsonSerializer.Serialize(blockHeader as BlockHeaderV1, options);
                    writer.WriteEndObject();
                }
            }
        }
    }

    /// <summary>
    /// A block header
    /// </summary>
    public class BlockHeaderV1 : BlockHeader
    {
        /// <summary>
        /// A seed needed for initializing a future era.
        /// </summary>
        [JsonPropertyName("accumulated_seed")]
        public string AccumulatedSeed { get; init; }

        /// <summary>
        /// The hash of the block's body.
        /// </summary>
        [JsonPropertyName("body_hash")]
        public string BodyHash { get; init; }

        /// <summary>
        /// The `EraEnd` of a block if it is a switch block.
        /// </summary>
        [JsonPropertyName("era_end")]
        public EraEndV1 EraEnd { get; init; }

        /// <summary>
        /// The era ID in which this block was created.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// The height of this block, i.e. the number of ancestors.
        /// </summary>
        [JsonPropertyName("height")]
        public ulong Height { get; init; }

        /// <summary>
        /// The parent block's hash.
        /// </summary>
        [JsonPropertyName("parent_hash")]
        public string ParentHash { get; init; }

        /// <summary>
        /// The protocol version of the network from when this block was created.
        /// </summary>
        [JsonPropertyName("protocol_version")]
        public string ProtocolVersion { get; init; }

        /// <summary>
        /// A random bit needed for initializing a future era.
        /// </summary>
        [JsonPropertyName("random_bit")]
        public bool RandomBit { get; init; }

        /// <summary>
        /// The root hash of global state after the deploys in this block have been executed.
        /// </summary>
        [JsonPropertyName("state_root_hash")]
        public string StateRootHash { get; init; }

        /// <summary>
        /// The timestamp from when the block was proposed.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; }
    }

    public class BlockHeaderV2 : BlockHeaderV1
    {
        /// <summary>
        /// The `EraEnd` of a block if it is a switch block.
        /// </summary>
        [JsonPropertyName("era_end")]
        public EraEndV2 EraEnd { get; init; }

        /// <summary>
        /// The gas price of the era.
        /// </summary>
        [JsonPropertyName("current_gas_price")]
        public UInt16 CurrentGasPrice { get; init; }
    }

    /// <summary>
    /// Validator that proposed the block
    /// </summary>
    public class Proposer
    {
        /// <summary>
        /// The block was proposed by the system, not a validator
        /// </summary>
        public bool IsSystem { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Replaced with IsSystem")]
        public bool isSystem
        {
            get => this.IsSystem;
            set => this.IsSystem = value;
        }

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
                            IsSystem = true,
                        };
                    return new Proposer()
                    {
                        IsSystem = false,
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
                writer.WriteStringValue(proposer.IsSystem ? "00" : proposer.PublicKey.ToAccountHex());
            }
        }
    }

    /// <summary>
    /// A block body
    /// </summary>
    public class BlockBodyV1
    {
        public virtual uint Version
        {
            get { return 1; }
        }

        /// <summary>
        /// The deploy hashes of the non-transfer deploys within the block.
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
        /// The deploy hashes of the transfers within the block.
        /// </summary>
        [JsonPropertyName("transfer_hashes")]
        public List<string> TransferHashes { get; init; }
    }

    /// <summary>
    /// A block body
    /// </summary>
    public class BlockBodyV2
    {
        /// <summary>
        /// The hashes of the installer/upgrader transactions within the block.
        /// </summary>
        [JsonPropertyName("install_upgrade")]
        public List<TransactionHash> InstallUpgrade { get; init; }

        /// <summary>
        /// The hashes of the auction transactions within the block.
        /// </summary>
        [JsonPropertyName("auction")]
        public List<TransactionHash> Auction { get; init; }

        /// <summary>
        /// The hashes of all other (non-installer/upgrader) transactions within the block.
        /// </summary>
        [JsonPropertyName("standard")]
        public List<TransactionHash> Standard { get; init; }

        /// <summary>
        /// Public key of the validator that proposed the block
        /// </summary>
        [JsonPropertyName("proposer")]
        [JsonConverter(typeof(Proposer.ProposerConverter))]
        public Proposer Proposer { get; init; }

        /// <summary>
        /// The hashes of the mint transactions within the block.
        /// </summary>
        [JsonPropertyName("mint")]
        public List<TransactionHash> Mint { get; init; }

        /// <summary>
        /// Describes finality signatures that will be rewarded in a block. Consists of a vector of
        /// `SingleBlockRewardedSignatures`, each of which describes signatures for a single ancestor block.
        /// The first entry represents the signatures for the parent block, the second for the parent of the parent,
        /// and so on.
        /// </summary>
        [JsonPropertyName("rewarded_signatures")]
        public List<List<UInt16>> RewardedSignatures { get; init; }
    }

    public interface IBlock
    {
        public int Version { get; }
        public BlockV1 BlockV1 { get; }
        public BlockV2 BlockV2 { get; }
        
        public string Hash { get; init; }
    }
    
    public abstract class Block : IBlock
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the block.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }

        /// <summary>
        /// Returns the block as a Version1 block object.
        /// </summary>
        BlockV1 IBlock.BlockV1 => this as BlockV1;

        /// <summary>
        /// Returns the block as a Version2 block object.
        /// </summary>
        BlockV2 IBlock.BlockV2 => this as BlockV2;

        /// <summary>
        /// Block hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; init; }

        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class BlockConverter : JsonConverter<IBlock>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(IBlock) ||
                       typeToConvert == typeof(Block);
            }

            public override IBlock Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    reader.Read();
                    var version = reader.GetString();
                    reader.Read();
                    switch (version)
                    {
                        case "Version1":
                        {
                            var block1 = JsonSerializer.Deserialize<BlockV1>(ref reader, options);
                            reader.Read();
                            block1._version = 1;
                            return block1;
                        }
                        case "Version2":
                            var block2 = JsonSerializer.Deserialize<BlockV2>(ref reader, options);
                            reader.Read();
                            block2._version = 2;
                            return block2;
                        default:
                            throw new JsonException("Expected Version1 or Version2");
                    }

                    ;
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                IBlock block,
                JsonSerializerOptions options)
            {
                switch (block.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(block as BlockV1, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(block as BlockV2, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected block version {block.Version}");
                }
            }
        }
    }

    /// <summary>
    /// A block in the network
    /// </summary>
    public class BlockV1 : Block
    {
        /// <summary>
        /// Block header
        /// </summary>
        [JsonPropertyName("header")]
        public BlockHeaderV1 Header { get; init; }

        /// <summary>
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBodyV1 Body { get; init; }
    }

    /// <summary>
    /// A block in the network
    /// </summary>
    public class BlockV2 : Block
    {
        /// <summary>
        /// Block header
        /// </summary>
        [JsonPropertyName("header")]
        public BlockHeaderV2 Header { get; init; }

        /// <summary>
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBodyV2 Body { get; init; }
    }

    /// <summary>
    /// A validator's public key paired with a corresponding signature of a given block hash.
    /// </summary>
    public class BlockProof
    {
        /// <summary>
        /// The validator's public key.
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// The validator's signature.
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }

    /// <summary>
    /// A JSON-friendly representation of a block and the signatures for that block
    /// </summary>
    public class BlockWithSignatures
    {
        /// <summary>
        /// The block.
        /// </summary>
        [JsonPropertyName("block")]
        [JsonConverter(typeof(Block.BlockConverter))]
        public IBlock Block { get; init; }

        /// <summary>
        /// The proofs of the block, i.e. a collection of validators' signatures of the block hash.
        /// </summary>
        [JsonPropertyName("proofs")]
        public List<BlockProof> Proofs { get; init; }
    }
}
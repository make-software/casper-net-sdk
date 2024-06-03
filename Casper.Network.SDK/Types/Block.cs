using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace Casper.Network.SDK.Types
{
   /// <summary>
    /// A block header
    /// </summary>
    public class BlockHeaderV1
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

    public class BlockHeader : BlockHeaderV1
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
        
        /// <summary>
        /// Public key of the validator that proposed the block
        /// </summary>
        [JsonPropertyName("proposer")]
        [JsonConverter(typeof(Proposer.ProposerConverter))]
        public Proposer Proposer { get; init; }
        
        [JsonPropertyName("last_switch_block_hash")]
        public string LastSwitchBlockHash { get; init; }
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
    public class BlockBody
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

    public class Block
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the block.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        protected BlockV1 _blockV1;
        
        // Explicit cast operator from A to B
        public static explicit operator BlockV1(Block block)
        {
            if(block._version == 1)
                return block._blockV1;

            throw new InvalidCastException("Version2 block cannot be converted to Version1");
        }
        
        public static explicit operator Block(BlockV1 block)
        {
            return new Block
            {
                _version = 1,
                _blockV1 = block,
                Hash = block.Hash,
                Header = BlockV1.BlockHeaderFromV1(block.Header, block.Body),
                Body = BlockV1.BlockBodyFromV1(block.Body),
            };
        }

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
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBody Body { get; init; }
        
        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class BlockConverter : JsonConverter<Block>
        {
            public override Block Read(
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
                            var blockv1 = JsonSerializer.Deserialize<BlockV1>(ref reader, options);
                            reader.Read();
                            return (Block)blockv1;
                        case "Version2":
                            var blockv2 = JsonSerializer.Deserialize<BlockV2>(ref reader, options);
                            reader.Read();
                            return new Block
                            {
                                _version = 2,
                                Hash = blockv2.Hash,
                                Header = blockv2.Header,
                                Body = blockv2.Body,
                            };
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
                Block block,
                JsonSerializerOptions options)
            {
                switch (block.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((BlockV1)block, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(block, options);
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
    public class BlockV1
    {
        /// <summary>
        /// Block hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; init; }
        
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

        internal static BlockHeader BlockHeaderFromV1(BlockHeaderV1 header, BlockBodyV1 body)
        {
            Dictionary<string, List<string>> rewards = new();
            if (header.EraEnd != null && header.EraEnd.EraReport.Rewards.Count > 0)
            {
                foreach (var r in header.EraEnd.EraReport.Rewards)
                {
                    rewards.Add(r.PublicKey.ToString().ToLower(), 
                        new List<string> { r.Amount.ToString()});
                }
            }
            
            EraEndV2 eraEnd = header.EraEnd == null ? null : new EraEndV2
            {
                NextEraValidatorWeights = header.EraEnd.NextEraValidatorWeights,
                NextEraGasPrice = 1,
                Equivocators = header.EraEnd.EraReport.Equivocators,
                InactiveValidators = header.EraEnd.EraReport.InactiveValidators,
                Rewards = rewards,
            };
            
            return new BlockHeader
            {
                BodyHash = header.BodyHash,
                ParentHash = header.ParentHash,
                Height = header.Height,
                Timestamp = header.Timestamp,
                EraId = header.EraId,
                RandomBit = header.RandomBit,
                AccumulatedSeed = header.AccumulatedSeed,
                StateRootHash = header.StateRootHash,
                ProtocolVersion = header.ProtocolVersion,
                CurrentGasPrice = 1,
                LastSwitchBlockHash = null,
                Proposer = body.Proposer,
                EraEnd = eraEnd,
            };
        }

        internal static BlockBody BlockBodyFromV1(BlockBodyV1 body)
        {
            var mintTransactions =
                body.TransferHashes.Select(t => new TransactionHash { Deploy = t }).ToList();
            var standardTransactions = 
                body.DeployHashes.Select(t => new TransactionHash { Deploy = t }).ToList();
            
            return new BlockBody
            {
                Auction = new List<TransactionHash>(),
                Mint = mintTransactions,
                Standard =  standardTransactions,
                InstallUpgrade = new List<TransactionHash>(),
                RewardedSignatures = null,
            };
        }
    }

    /// <summary>
    /// A block in the network
    /// </summary>
    internal class BlockV2
    {
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
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBody Body { get; init; }
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
}
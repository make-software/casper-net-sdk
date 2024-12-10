using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public class BlockHeaderV2 : BlockHeaderV1
    {
        /// <summary>
        /// The `EraEnd` of a block if it is a switch block.
        /// </summary>
        [JsonPropertyName("era_end")]
        public EraEnd EraEnd { get; init; }

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
    /// A block header (alias for BlockHeaderV2)
    /// </summary>
    public class BlockHeader
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the block.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        protected BlockHeaderV1 _blockHeaderV1;

        protected BlockHeaderV2 _blockHeaderV2;
        
        /// <summary>
        /// A seed needed for initializing a future era.
        /// </summary>
        public string AccumulatedSeed { get; init; }

        /// <summary>
        /// The hash of the block's body.
        /// </summary>
        public string BodyHash { get; init; }

        /// <summary>
        /// The era ID in which this block was created.
        /// </summary>
        public ulong EraId { get; init; }

        /// <summary>
        /// The height of this block, i.e. the number of ancestors.
        /// </summary>
        public ulong Height { get; init; }

        /// <summary>
        /// The parent block's hash.
        /// </summary>
        public string ParentHash { get; init; }

        /// <summary>
        /// The protocol version of the network from when this block was created.
        /// </summary>
        public string ProtocolVersion { get; init; }

        /// <summary>
        /// A random bit needed for initializing a future era.
        /// </summary>
        public bool RandomBit { get; init; }

        /// <summary>
        /// The root hash of global state after the deploys in this block have been executed.
        /// </summary>
        public string StateRootHash { get; init; }

        /// <summary>
        /// The timestamp from when the block was proposed.
        /// </summary>
        public string Timestamp { get; init; }
        
        /// <summary>
        /// The `EraEnd` of a block if it is a switch block.
        /// </summary>
        public EraEnd EraEnd { get; init; }

        /// <summary>
        /// The gas price of the era.
        /// </summary>
        public UInt16 CurrentGasPrice { get; init; }
        
        /// <summary>
        /// Public key of the validator that proposed the block
        /// </summary>
        public Proposer Proposer { get; init; }
        
        public string LastSwitchBlockHash { get; init; }
        
        public class BlockHeaderConverter : JsonConverter<BlockHeader>
        {
            public override BlockHeader Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                    {
                        if (document.RootElement.TryGetProperty("parent_hash", out var header))
                        {
                            var blockHeaderV1 = JsonSerializer.Deserialize<BlockHeaderV1>(document.RootElement.GetRawText());
                            return (BlockHeader)blockHeaderV1;
                        }
                        if (document.RootElement.TryGetProperty("Version1", out var headerV1))
                        {
                            var blockHeaderV1 = JsonSerializer.Deserialize<BlockHeaderV1>(headerV1.GetRawText());
                            return (BlockHeader)blockHeaderV1;
                        }
                        if (document.RootElement.TryGetProperty("Version2", out var headerV2))
                        {
                            var blockHeaderV2 = JsonSerializer.Deserialize<BlockHeaderV2>(headerV1.GetRawText());
                            return (BlockHeader)blockHeaderV2;
                        }
                        throw new JsonException("Cannot deserialize BlockHeader");
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
                switch (blockHeader.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((BlockHeaderV1)blockHeader, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((BlockHeaderV2)blockHeader, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected block header version {blockHeader.Version}");
                }
            }
        }
        
        public static explicit operator BlockHeaderV1(BlockHeader blockHeader)
        {
            if(blockHeader._version == 1)
                return blockHeader._blockHeaderV1;

            throw new InvalidCastException("Version2 block header cannot be converted to Version1");
        }
        
        public static explicit operator BlockHeaderV2(BlockHeader blockHeader)
        {
            if(blockHeader._version == 2)
                return blockHeader._blockHeaderV2;

            throw new InvalidCastException("Version1 block header cannot be converted to Version2");
        }
        
        public static explicit operator BlockHeader(BlockHeaderV1 blockHeader)
        {
            return new BlockHeader
            {
                _version = 1,
                _blockHeaderV1 = blockHeader,

                AccumulatedSeed = blockHeader.AccumulatedSeed,
                EraId = blockHeader.EraId,
                Height = blockHeader.Height,
                ParentHash = blockHeader.ParentHash,
                ProtocolVersion = blockHeader.ProtocolVersion,
                StateRootHash = blockHeader.StateRootHash,
                Timestamp = blockHeader.Timestamp,
                RandomBit = blockHeader.RandomBit,
                CurrentGasPrice = 1,
                LastSwitchBlockHash = null,
                Proposer = null,
                EraEnd = BlockV1.EraEndFromV1(blockHeader),
            };
        }
        
        public static explicit operator BlockHeader(BlockHeaderV2 blockHeader)
        {
            return new BlockHeader
            {
                _version = 2,
                _blockHeaderV2 = blockHeader,

                AccumulatedSeed = blockHeader.AccumulatedSeed,
                EraId = blockHeader.EraId,
                Height = blockHeader.Height,
                ParentHash = blockHeader.ParentHash,
                ProtocolVersion = blockHeader.ProtocolVersion,
                StateRootHash = blockHeader.StateRootHash,
                Timestamp = blockHeader.Timestamp,
                RandomBit = blockHeader.RandomBit,
                CurrentGasPrice = blockHeader.CurrentGasPrice,
                LastSwitchBlockHash = blockHeader.LastSwitchBlockHash,
                Proposer = blockHeader.Proposer,
                EraEnd = blockHeader.EraEnd,
            };
        }
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
    public class BlockBodyV2
    {
        [JsonPropertyName("transactions")]
        [JsonConverter(typeof(BlockTransaction.BlockTransactionConverter))]
        public List<BlockTransaction> Transactions { get; init; }
        // public Dictionary<string,List<TransactionHash>> Transactions { get; init; }
        
        /// <summary>
        /// Describes finality signatures that will be rewarded in a block. Consists of a vector of
        /// `SingleBlockRewardedSignatures`, each of which describes signatures for a single ancestor block.
        /// The first entry represents the signatures for the parent block, the second for the parent of the parent,
        /// and so on.
        /// </summary>
        [JsonPropertyName("rewarded_signatures")]
        public List<List<UInt16>> RewardedSignatures { get; init; }
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

        internal static EraEnd EraEndFromV1(BlockHeaderV1 header)
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
            
            EraEnd eraEnd = header.EraEnd == null ? null : new EraEnd
            {
                NextEraValidatorWeights = header.EraEnd.NextEraValidatorWeights,
                NextEraGasPrice = 1,
                Equivocators = header.EraEnd.EraReport.Equivocators,
                InactiveValidators = header.EraEnd.EraReport.InactiveValidators,
                Rewards = rewards,
            };
            return eraEnd;
        }

        internal static List<BlockTransaction> BlockTransactionsFromV1(BlockBodyV1 body)
        {
            var txs = new List<BlockTransaction>();
            
            var mintTransactions =
                body.TransferHashes.Select(hash => new BlockTransaction()
                {
                    Category = TransactionCategory.Mint,
                    Hash = hash,
                    Version = TransactionVersion.Deploy,
                }).ToList();
            if(mintTransactions.Count > 0)
                txs.AddRange(mintTransactions);
            
            var standardTransactions = 
                body.DeployHashes.Select(hash => new BlockTransaction()
                {
                    Category = TransactionCategory.Large,
                    Hash = hash,
                    Version = TransactionVersion.Deploy,
                }).ToList();
            if(standardTransactions.Count > 0)
                txs.AddRange(standardTransactions);

            return txs;
        }
    }

    /// <summary>
    /// A block in the network
    /// </summary>
    public class BlockV2
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
        public BlockHeaderV2 Header { get; init; }

        /// <summary>
        /// Block body
        /// </summary>
        [JsonPropertyName("body")]
        public BlockBodyV2 Body { get; init; }
    }
    
    public enum TransactionCategory {
        /// <summary>
        /// Native mint interaction (the default).
        /// </summary>
        Mint = 0,
        /// <summary>
        /// Transfer from a Deploy transaction.
        /// </summary>
        [Obsolete("Use Mint instead of DeployTransfer")]
        DeployTransfer = 0,
        /// <summary>
        /// Native auction interaction.
        /// </summary>
        Auction = 1,
        /// <summary>
        /// Install or Upgrade.
        /// </summary>
        InstallUpgrade = 2,
        /// <summary>
        /// A large Wasm based transaction.
        /// </summary>
        Large = 3,
        /// <summary>
        /// A non-native Deploy transaction.
        /// </summary>
        [Obsolete("Use Large instead of DeployLarge")]
        DeployLarge = 3,
        /// <summary>
        /// A medium Wasm based transaction.
        /// </summary>
        Medium = 4,
        /// <summary>
        /// A small Wasm based transaction.
        /// </summary>
        Small = 5,
    }

    public enum TransactionVersion
    {
        Deploy = 0,
        TransactionV1 = 1,
    }
    
    public class BlockTransaction
    {
        public TransactionCategory Category { get; init; }
        public TransactionVersion Version { get; init; }
        public string Hash { get; init; }
        
        public class BlockTransactionConverter : JsonConverter<List<BlockTransaction>>
        {
            public override List<BlockTransaction> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var txs = new List<BlockTransaction>();
                
                try
                {
                    reader.Read(); // skip start object
                    while (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var categoryNumber = reader.GetString();
                        var category = EnumCompat.Parse<TransactionCategory>(categoryNumber);
                        reader.Read();
                        var list = JsonSerializer.Deserialize<List<TransactionHash>>(ref reader, options);
                        reader.Read();
                        if (list.Count > 0)
                        {
                            txs.AddRange(list.Select(t => new BlockTransaction()
                            {
                                Category = category,
                                Hash = t.Deploy != null ? t.Deploy : t.Version1,
                                Version = t.Deploy != null ? TransactionVersion.Deploy : TransactionVersion.TransactionV1,
                            }));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }

                return txs;
            }

            public override void Write(
                Utf8JsonWriter writer,
                List<BlockTransaction> block,
                JsonSerializerOptions options)
            {
                throw new JsonException($"Serialization of BlockTransaction not available");
            }
        }
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

        protected BlockV2 _blockV2;
        
        /// <summary>
        /// Block hash
        /// </summary>
        public string Hash { get; init; }

        /// <summary>
        /// A seed needed for initializing a future era.
        /// </summary>
        public string AccumulatedSeed { get; init; }

        /// <summary>
        /// The era ID in which this block was created.
        /// </summary>
        public ulong EraId { get; init; }

        /// <summary>
        /// The height of this block, i.e. the number of ancestors.
        /// </summary>
        public ulong Height { get; init; }

        /// <summary>
        /// The parent block's hash.
        /// </summary>
        public string ParentHash { get; init; }

        /// <summary>
        /// The protocol version of the network from when this block was created.
        /// </summary>
        public string ProtocolVersion { get; init; }

        /// <summary>
        /// A random bit needed for initializing a future era.
        /// </summary>
        public bool RandomBit { get; init; }

        /// <summary>
        /// The root hash of global state after the deploys in this block have been executed.
        /// </summary>
        public string StateRootHash { get; init; }

        /// <summary>
        /// The timestamp from when the block was proposed.
        /// </summary>
        public string Timestamp { get; init; }
        
        /// <summary>
        /// The `EraEnd` of a block if it is a switch block.
        /// </summary>
        public EraEnd EraEnd { get; init; }

        /// <summary>
        /// The gas price of the era.
        /// </summary>
        public UInt16 CurrentGasPrice { get; init; }
        
        /// <summary>
        /// Public key of the validator that proposed the block
        /// </summary>
        public Proposer Proposer { get; init; }
        
        public string LastSwitchBlockHash { get; init; }

        public List<BlockTransaction> Transactions { get; init; }
        
        /// <summary>
        /// Describes finality signatures that will be rewarded in a block. Consists of a vector of
        /// `SingleBlockRewardedSignatures`, each of which describes signatures for a single ancestor block.
        /// The first entry represents the signatures for the parent block, the second for the parent of the parent,
        /// and so on.
        /// </summary>
        public List<List<UInt16>> RewardedSignatures { get; init; }
        
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
                            return (Block)blockv2;
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
                        JsonSerializer.Serialize(writer, (BlockV1)block, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(writer, block, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected block version {block.Version}");
                }
            }
        }
        
        public static explicit operator BlockV1(Block block)
        {
            if(block._version == 1)
                return block._blockV1;

            throw new InvalidCastException("Version2 block cannot be converted to Version1");
        }
        
        public static explicit operator BlockV2(Block block)
        {
            if(block._version == 2)
                return block._blockV2;

            throw new InvalidCastException("Version1 block cannot be converted to Version2");
        }
        
        public static explicit operator Block(BlockV1 block)
        {
            return new Block
            {
                _version = 1,
                _blockV1 = block,

                Hash = block.Hash,
                
                AccumulatedSeed = block.Header.AccumulatedSeed,
                EraId = block.Header.EraId,
                Height = block.Header.Height,
                ParentHash = block.Header.ParentHash,
                ProtocolVersion = block.Header.ProtocolVersion,
                StateRootHash = block.Header.StateRootHash,
                Timestamp = block.Header.Timestamp,
                RandomBit = block.Header.RandomBit,
                CurrentGasPrice = 1,
                LastSwitchBlockHash = null,
                Proposer = block.Body.Proposer,
                EraEnd = BlockV1.EraEndFromV1(block.Header),

                Transactions = BlockV1.BlockTransactionsFromV1(block.Body),
                RewardedSignatures = null,
            };
        }
        
        public static explicit operator Block(BlockV2 block)
        {
            return new Block
            {
                _version = 2,
                _blockV2 = block,

                Hash = block.Hash,
                
                AccumulatedSeed = block.Header.AccumulatedSeed,
                EraId = block.Header.EraId,
                Height = block.Header.Height,
                ParentHash = block.Header.ParentHash,
                ProtocolVersion = block.Header.ProtocolVersion,
                StateRootHash = block.Header.StateRootHash,
                Timestamp = block.Header.Timestamp,
                RandomBit = block.Header.RandomBit,
                CurrentGasPrice = block.Header.CurrentGasPrice,
                LastSwitchBlockHash = block.Header.LastSwitchBlockHash,
                Proposer = block.Header.Proposer,
                EraEnd = block.Header.EraEnd,

                Transactions = block.Body.Transactions,
                RewardedSignatures = block.Body.RewardedSignatures,
            };
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Keys in the global state store information about different data types.
    /// <see cref="https://casper.network/docs/design/serialization-standard#serialization-standard-state-keys"/>
    /// </summary>
    public enum KeyIdentifier
    {
        /// <summary>
        /// AccountHash keys store accounts in the global state. 
        /// </summary>
        Account = 0x00,
        /// <summary>
        /// Hash keys store contracts immutably in the global state.
        /// </summary>
        Hash = 0x01,
        /// <summary>
        /// URef keys store values and manage permissions to interact with the value stored under the URef.
        /// </summary>
        URef = 0x02,
        /// <summary>
        /// Transfer keys store transfers in the global state.
        /// </summary>
        Transfer = 0x03,
        /// <summary>
        /// DeployInfo keys store information related to deploys in the global state.
        /// </summary>
        DeployInfo = 0x04,
        /// <summary>
        /// EraInfo keys store information related to the Auction metadata for a particular era.
        /// </summary>
        EraInfo = 0x05,
        /// <summary>
        /// Balance keys store information related to the balance of a given purse.
        /// </summary>
        Balance = 0x06,
        /// <summary>
        /// Bid keys store information related to auction bids in the global state.
        /// </summary>
        Bid = 0x07,
        /// <summary>
        /// Withdraw keys store information related to auction withdraws in the global state.
        /// </summary>
        Withdraw = 0x08,
        /// <summary>
        /// Dictionary keys store dictionary items.
        /// </summary>
        Dictionary = 0x09,
        /// <summary>
        /// A `Key` variant under which system contract hashes are stored.
        /// </summary>
        SystemContractRegistry = 0x0a,
        /// <summary>
        /// A `Key` under which we store unbond information.
        /// </summary>
        Unbond = 0x0b,
        /// <summary>
        /// A `Key` variant under which chainspec and other hashes are stored.
        /// </summary>
        ChainspecRegistry = 0x0c,
        /// <summary>
        /// A `Key` variant under which we store the root hash of a Merkle tree containing the
        /// execution results for the block at this height.
        /// </summary>
        BlockEffectsRootHash = 0x0d,
        /// <summary>
        /// A `Key` variant under which we store the root hash of a Merkle tree containing the
        /// approvals for all the deploys in the block at this height.
        /// </summary>
        DeployApprovalsRootHash = 0x0e,
    }

    /// <summary>
    /// Base class for the different global state keys. 
    /// </summary>
    public abstract class GlobalStateKey
    {
        protected readonly string Key;

        public KeyIdentifier KeyIdentifier { get; init; }

        protected virtual byte[] _GetRawBytesFromKey(string key) =>
            Hex.Decode(key.Substring(key.LastIndexOf('-') + 1));

        public byte[] RawBytes
        {
            get { return _GetRawBytesFromKey(Key); }
        }

        protected GlobalStateKey(string key)
        {
            Key = key;
        }
        
        protected GlobalStateKey(string key, string keyPrefix)
        {
            if (!key.StartsWith(keyPrefix))
                throw new ArgumentException($"Key not valid. It should start with '{keyPrefix}'.",
                    nameof(key));

            var bytes = CEP57Checksum.Decode(key.Substring(key.LastIndexOf('-') + 1), out int checksumResult);
            if (checksumResult == CEP57Checksum.InvalidChecksum)
                throw new ArgumentException("Global State Key checksum mismatch.");
            
            Key = keyPrefix + CEP57Checksum.Encode(bytes);
        }

        public string ToHexString()
        {
            return CEP57Checksum.Encode(RawBytes);
        }

        /// <summary>
        /// Converts a global state key from string to its specific key object. 
        /// </summary>
        public static GlobalStateKey FromString(string value)
        {
            if (value.StartsWith("account-hash-"))
                return new AccountHashKey(value);
            if (value.StartsWith("hash-"))
                return new HashKey(value);
            if (value.StartsWith("contract-package-wasm"))
                return new HashKey(value.Replace("contract-package-wasm", "hash-"));
            if (value.StartsWith("contract-package-"))
                return new HashKey(value.Replace("contract-package-", "hash-"));
            if (value.StartsWith("contract-wasm-"))
                return new HashKey(value.Replace("contract-wasm-", "hash-"));
            if (value.StartsWith("contract-"))
                return new HashKey(value.Replace("contract-", "hash-"));
            if (value.StartsWith("uref-"))
                return new URef(value);
            if (value.StartsWith("transfer-"))
                return new TransferKey(value);
            if (value.StartsWith("era-"))
                return new EraInfoKey(value);
            if (value.StartsWith("balance-"))
                return new BalanceKey(value);
            if (value.StartsWith("bid"))
                return new BidKey(value);
            if (value.StartsWith("withdraw"))
                return new WithdrawKey(value);
            if (value.StartsWith("dictionary"))
                return new DictionaryKey(value);
            if (value.StartsWith("system-contract-registry-"))
                return new SystemContractRegistryKey(value);
            if (value.StartsWith("unbond-"))
                return new UnbondKey(value);
            if (value.StartsWith("chainspec-registry-"))
                return new ChainspecRegistryKey(value);
            if (value.StartsWith("block-effects-root-hash-"))
                return new BlockEffectsRootHashKey(value);
            if (value.StartsWith("deploy-approvals-root-hash-"))
                return new DeployApprovalsRootHashKey(value);
            if (value.StartsWith("deploy-"))
                return new DeployInfoKey(value);
            
            throw new ArgumentException($"Key not valid. Unknown key prefix in \"{value}\".");
        }

        /// <summary>
        /// Converts a global state key from a byte array to its specific key object. First
        /// byte in the array indicates the Key identifier.
        /// </summary>
        public static GlobalStateKey FromBytes(byte[] bytes)
        {
            return bytes[0] switch
            {
                0x00 => new AccountHashKey("account-hash-" + CEP57Checksum.Encode(bytes[1..])),
                0x01 => new HashKey("hash-" + CEP57Checksum.Encode(bytes[1..])),
                0x02 => new URef(bytes[1..]),
                0x03 => new TransferKey("transfer-" + CEP57Checksum.Encode(bytes[1..])),
                0x04 => new DeployInfoKey("deploy-" + CEP57Checksum.Encode(bytes[1..])),
                0x05 => new EraInfoKey("era-" + BitConverter.ToInt64(bytes, 1)),
                0x06 => new BalanceKey("balance-" + CEP57Checksum.Encode(bytes[1..])),
                0x07 => new BidKey("bid-" + CEP57Checksum.Encode(bytes[1..])),
                0x08 => new WithdrawKey("withdraw-" + CEP57Checksum.Encode(bytes[1..])),
                0x09 => new DictionaryKey("dictionary-" + CEP57Checksum.Encode(bytes[1..])),
                0x0a => new SystemContractRegistryKey("system-contract-registry-" + CEP57Checksum.Encode(bytes[1..])),
                0x0b => new UnbondKey("unbond-" + CEP57Checksum.Encode(bytes[1..])),
                0x0c => new ChainspecRegistryKey("chainspec-registry-" + CEP57Checksum.Encode(bytes[1..])),
                0x0d => new BlockEffectsRootHashKey("block-effects-root-hash-" + BitConverter.ToInt64(bytes, 1)),
                0x0e => new DeployApprovalsRootHashKey("deploy-approvals-root-hash-" + BitConverter.ToInt64(bytes, 1)),
                _ => throw new ArgumentException($"Unknown key identifier '{bytes[0]}'")
            };
        }

        public virtual byte[] GetBytes()
        {
            var ms = new MemoryStream();
            ms.WriteByte((byte)this.KeyIdentifier);
            ms.Write(this.RawBytes);

            return ms.ToArray();
        }

        /// <summary>
        /// Converts a key object to a string with the right prefix 
        /// </summary>
        public override string ToString()
        {
            return Key;
        }

        /// <summary>
        /// Json converter class to serialize/deserialize an object derived from
        /// GlobalStateKey to/from Json
        /// </summary>
        public class GlobalStateKeyConverter : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(GlobalStateKey) ||
                       typeToConvert == typeof(AccountHashKey) ||
                       typeToConvert == typeof(HashKey) ||
                       typeToConvert == typeof(URef) ||
                       typeToConvert == typeof(TransferKey) ||
                       typeToConvert == typeof(DeployInfoKey) ||
                       typeToConvert == typeof(EraInfoKey) ||
                       typeToConvert == typeof(BalanceKey) ||
                       typeToConvert == typeof(BidKey) ||
                       typeToConvert == typeof(WithdrawKey) ||
                       typeToConvert == typeof(DictionaryKey) ||
                       typeToConvert == typeof(SystemContractRegistryKey) ||
                       typeToConvert == typeof(UnbondKey) ||
                       typeToConvert == typeof(ChainspecRegistryKey) ||
                       typeToConvert == typeof(BlockEffectsRootHashKey) ||
                       typeToConvert == typeof(DeployApprovalsRootHashKey);
            }

            public override JsonConverter CreateConverter(
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                return (JsonConverter) Activator.CreateInstance(
                    typeof(GlobalStateKeyConverterInner),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: new object[] {options},
                    culture: null);
            }

            public class GlobalStateKeyConverterInner : JsonConverter<GlobalStateKey>
            {
                public GlobalStateKeyConverterInner(JsonSerializerOptions options)
                {
                }

                public override GlobalStateKey Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options)
                {
                    {
                        try
                        {
                            return  GlobalStateKey.FromString(reader.GetString());
                        }
                        catch (Exception e)
                        {
                            throw new JsonException(e.Message);
                        }
                    }
                }

                public override void Write(
                    Utf8JsonWriter writer,
                    GlobalStateKey value,
                    JsonSerializerOptions options) =>
                    writer.WriteStringValue(value.ToString());
            }
        }
    }

    /// <summary>
    /// Stores an account in the global state.
    /// Format: 32-byte length with prefix 'account-hash-'.
    /// </summary>
    public class AccountHashKey : GlobalStateKey
    {
        public static string KEYPREFIX = "account-hash-";

        public AccountHashKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Account;
        }

        public AccountHashKey(PublicKey publicKey)
            : base(publicKey.GetAccountHash(), KEYPREFIX)
        {
        }
    }

    /// <summary>
    /// Stores a contract inmutably in the global state.
    /// Format: 32-byte length with prefix 'hash-'.
    /// </summary>
    public class HashKey : GlobalStateKey
    {
        public static string KEYPREFIX = "hash-";

        public HashKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Hash;
        }

        public HashKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information for a transfer in the global state.
    /// Format: 32-byte length with prefix 'transfer-'.
    /// </summary>
    public class TransferKey : GlobalStateKey
    {
        public static string KEYPREFIX = "transfer-";

        public TransferKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Transfer;
        }
        
        public TransferKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information for a Deploy in the global state.
    /// Format: 32-byte length with prefix 'deploy-'.
    /// </summary>
    public class DeployInfoKey : GlobalStateKey
    {
        public static string KEYPREFIX = "deploy-";

        public DeployInfoKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.DeployInfo;
        }
        
        public DeployInfoKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    public abstract class U64GlobalStateKey : GlobalStateKey
    {
        protected U64GlobalStateKey(string key) : base(key)
        {
        }
        
        protected override byte[] _GetRawBytesFromKey(string key)
        {
            var u64 = ulong.Parse(key.Split('-').Last());
            byte[] bytes = BitConverter.GetBytes(u64);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return bytes;
        }

        public override byte[] GetBytes()
        {
            var ms = new MemoryStream(9);
            ms.WriteByte((byte)this.KeyIdentifier);
            ms.Write(this.RawBytes);

            return ms.ToArray();
        }
    }
    
    /// <summary>
    /// Stores information related to the Auction metadata for a particular era..
    /// Format: u64 number with prefix 'era-' (e.g. 'era-3407').
    /// </summary>
    public class EraInfoKey : U64GlobalStateKey
    {
        public static string KEYPREFIX = "era-";

        public EraInfoKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.EraInfo;
            
            if (!key.StartsWith(KEYPREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{KEYPREFIX}'.",
                    nameof(key));
            
            if(!long.TryParse(key.Substring(KEYPREFIX.Length), out var eraNum))
                throw new ArgumentException($"Key not valid. Cannot parse era number.",
                    nameof(key));
        }
    }

    /// <summary>
    /// Stores information related to the balance of a given purse.
    /// Format: 32-byte length with prefix 'balance-'.
    /// </summary>
    public class BalanceKey : GlobalStateKey
    {
        public static string KEYPREFIX = "balance-";

        public BalanceKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Balance;
        }
        
        public BalanceKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information related to auction bids in the global state.
    /// Format: 32-byte length with prefix 'bid-'.
    /// </summary>
    public class BidKey : GlobalStateKey
    {
        public static string KEYPREFIX = "bid-";

        public BidKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Bid;
        }
        
        public BidKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information related to auction withdraws in the global state.
    /// Format: 32-byte length with prefix 'withdraw-'.
    /// </summary>
    public class WithdrawKey : GlobalStateKey
    {
        public static string KEYPREFIX = "withdraw-";

        public WithdrawKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Withdraw;
        }
        
        public WithdrawKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores dictionary items in the global state.
    /// Format: 32-byte length with prefix 'dictionary-'.
    /// </summary>
    public class DictionaryKey : GlobalStateKey
    {
        public static string KEYPREFIX = "dictionary-";

        public DictionaryKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Dictionary;
        }

        public DictionaryKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores a list of system contract hashes (auction,
    /// handle payment, mint and standard payment).
    /// Format: 32-byte length with prefix 'system-contract-registry-'.
    /// </summary>
    public class SystemContractRegistryKey : GlobalStateKey
    {
        public static string KEYPREFIX = "system-contract-registry-";

        public SystemContractRegistryKey() : this(
            KEYPREFIX + "0000000000000000000000000000000000000000000000000000000000000000")
        {
        }
        
        public SystemContractRegistryKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.SystemContractRegistry;
        }

        public SystemContractRegistryKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }
    
    /// <summary>
    /// Stores unbond information in the global state.
    /// Format: 32-byte length with prefix 'unbond-'.
    /// </summary>
    public class UnbondKey : GlobalStateKey
    {
        public static string KEYPREFIX = "unbond-";

        public UnbondKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Unbond;
        }

        public UnbondKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores a mapping of file names to the hash of the file itself. These files include *Chainspec.toml* and may also include *Accounts.toml* and *GlobalState.toml*.
    /// Format: 32-byte length with prefix 'chainspec-registry-'.
    /// </summary>
    public class ChainspecRegistryKey : GlobalStateKey
    {
        public static string KEYPREFIX = "chainspec-registry-";

        public ChainspecRegistryKey() : this(
            KEYPREFIX + "1111111111111111111111111111111111111111111111111111111111111111")
        {
        }
        
        public ChainspecRegistryKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.ChainspecRegistry;
        }

        public ChainspecRegistryKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }
    
    /// <summary>
    /// Stores the root hash of a Merkle tree containing the execution results for the block at this height.
    /// Format: u64 number with prefix 'block-effects-root-hash-' (e.g. 'block-effects-root-hash-1125463').
    /// Note: the key exists only when the block at this height contains at least one deploy.
    /// </summary>
    public class BlockEffectsRootHashKey : U64GlobalStateKey
    {
        public static string KEYPREFIX = "block-effects-root-hash-";
        
        public BlockEffectsRootHashKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.BlockEffectsRootHash;
            
            if (!key.StartsWith(KEYPREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{KEYPREFIX}'.",
                    nameof(key));
            
            if(!long.TryParse(key.Substring(KEYPREFIX.Length), out var eraNum))
                throw new ArgumentException($"Key not valid. Cannot parse {KEYPREFIX}key number.",
                    nameof(key));
        }
    }
    
    /// <summary>
    /// Stores the root hash of a Merkle tree containing the approvals for all the deploys in the block at this height. 
    /// Format: u64 number with prefix 'deploy-approvals-root-hash-' (e.g. 'deploy-approvals-root-hash-1125463').
    /// Note: the key exists only when the block at this height contains at least one deploy.
    /// </summary>
    public class DeployApprovalsRootHashKey : U64GlobalStateKey
    {
        public static string KEYPREFIX = "deploy-approvals-root-hash-";

        public DeployApprovalsRootHashKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.DeployApprovalsRootHash;
            
            if (!key.StartsWith(KEYPREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{KEYPREFIX}'.",
                    nameof(key));
            
            if(!long.TryParse(key.Substring(KEYPREFIX.Length), out var eraNum))
                throw new ArgumentException($"Key not valid. Cannot parse {KEYPREFIX}key number.",
                    nameof(key));
        }
    }
}

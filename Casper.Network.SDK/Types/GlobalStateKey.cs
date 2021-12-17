using System;
using System.IO;
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
        Dictionary = 0x09
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
            if (value.StartsWith("contract-wasm-"))
                return new HashKey(value.Replace("contract-wasm-", "hash-"));
            if (value.StartsWith("contract-"))
                return new HashKey(value.Replace("contract-", "hash-"));
            if (value.StartsWith("uref-"))
                return new URef(value);
            if (value.StartsWith("transfer-"))
                return new TransferKey(value);
            if (value.StartsWith("deploy-"))
                return new DeployInfoKey(value);
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

            throw new ArgumentException($"Key not valid. Unknown key prefix.");
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
                       typeToConvert == typeof(DictionaryKey);
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
        public AccountHashKey(string key) : base(key, "account-hash-")
        {
            KeyIdentifier = KeyIdentifier.Account;
        }

        public AccountHashKey(PublicKey publicKey)
            : base(publicKey.GetAccountHash(), "account-hash-")
        {
        }
    }

    /// <summary>
    /// Stores a contract inmutably in the global state.
    /// Format: 32-byte length with prefix 'hash-'.
    /// </summary>
    public class HashKey : GlobalStateKey
    {
        public HashKey(string key) : base(key, "hash-")
        {
            KeyIdentifier = KeyIdentifier.Hash;
        }

        public HashKey(byte[] key) : this("hash-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information for a transfer in the global state.
    /// Format: 32-byte length with prefix 'transfer-'.
    /// </summary>
    public class TransferKey : GlobalStateKey
    {
        public TransferKey(string key) : base(key, "transfer-")
        {
            KeyIdentifier = KeyIdentifier.Transfer;
        }
        
        public TransferKey(byte[] key) : this("transfer-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information for a Deploy in the global state.
    /// Format: 32-byte length with prefix 'deploy-'.
    /// </summary>
    public class DeployInfoKey : GlobalStateKey
    {
        public DeployInfoKey(string key) : base(key, "deploy-")
        {
            KeyIdentifier = KeyIdentifier.DeployInfo;
        }
        
        public DeployInfoKey(byte[] key) : this("deploy-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information related to the Auction metadata for a particular era..
    /// Format: u64 number with prefix 'era-' (e.g. 'era-3407').
    /// </summary>
    public class EraInfoKey : GlobalStateKey
    {
        public EraInfoKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.EraInfo;
            
            if (!key.StartsWith("era-"))
                throw new ArgumentException($"Key not valid. It should start with 'era-'.",
                    nameof(key));
            
            if(!long.TryParse(key.Substring(4), out var eraNum))
                throw new ArgumentException($"Key not valid. Cannot parse era number.",
                    nameof(key));
        }

        protected override byte[] _GetRawBytesFromKey(string key)
        {
            var u64 = ulong.Parse(key.Substring(4));
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
    /// Stores information related to the balance of a given purse.
    /// Format: 32-byte length with prefix 'balance-'.
    /// </summary>
    public class BalanceKey : GlobalStateKey
    {
        public BalanceKey(string key) : base(key, "balance-")
        {
            KeyIdentifier = KeyIdentifier.Balance;
        }
        
        public BalanceKey(byte[] key) : this("balance-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information related to auction bids in the global state.
    /// Format: 32-byte length with prefix 'bid-'.
    /// </summary>
    public class BidKey : GlobalStateKey
    {
        public BidKey(string key) : base(key, "bid-")
        {
            KeyIdentifier = KeyIdentifier.Bid;
        }
        
        public BidKey(byte[] key) : this("bid-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores information related to auction withdraws in the global state.
    /// Format: 32-byte length with prefix 'withdraw-'.
    /// </summary>
    public class WithdrawKey : GlobalStateKey
    {
        public WithdrawKey(string key) : base(key, "withdraw-")
        {
            KeyIdentifier = KeyIdentifier.Withdraw;
        }
        
        public WithdrawKey(byte[] key) : this("withdraw-" + CEP57Checksum.Encode(key))
        {
        }
    }

    /// <summary>
    /// Stores dictionary items in the global state.
    /// Format: 32-byte length with prefix 'dictionary-'.
    /// </summary>
    public class DictionaryKey : GlobalStateKey
    {
        public DictionaryKey(string key) : base(key, "dictionary-")
        {
            KeyIdentifier = KeyIdentifier.Dictionary;
        }

        public DictionaryKey(byte[] key) : this("dictionary-" + CEP57Checksum.Encode(key))
        {
        }
    }
}
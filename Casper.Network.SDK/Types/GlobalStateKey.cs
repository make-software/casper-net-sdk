using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum KeyIdentifier
    {
        Account = 0x00,
        Hash = 0x01,
        URef = 0x02,
        Transfer = 0x03,
        DeployInfo = 0x04,
        EraInfo = 0x05,
        Balance = 0x06,
        Bid = 0x07,
        Withdraw = 0x08,
        Dictionary = 0x09
    }

    public abstract class GlobalStateKey
    {
        protected string Key;

        public KeyIdentifier KeyIdentifier { get; init; }

        public byte[] RawBytes { get { return _GetRawBytesFromKey(Key); } }

        protected GlobalStateKey(string key, string keyPrefix)
        {
            if (!key.StartsWith(keyPrefix))
                throw new ArgumentException($"Key not valid. It should start with '{keyPrefix}'.",
                    nameof(key));

            Key = key;
        }

        protected virtual byte[] _GetRawBytesFromKey(string key)
        {
            return Hex.Decode(key.Substring(key.LastIndexOf('-') + 1));
        }

        public static GlobalStateKey FromString(string value)
        {
            if (value.StartsWith("account-hash-"))
                return new AccountHashKey(value);
            if (value.StartsWith("hash-"))
                return new HashKey(value);
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

            return null;
        }

        public static GlobalStateKey FromBytes(byte[] bytes)
        {
            return bytes[0] switch
            {
                0x00 => new AccountHashKey("account-hash-" + Hex.ToHexString(bytes[1..])),
                0x01 => new HashKey("hash-" + Hex.ToHexString(bytes[1..])),
                0x02 => new URef(bytes[1..]),
                0x03 => new TransferKey("transfer-" + Hex.ToHexString(bytes[1..])),
                0x04 => new DeployInfoKey("deploy-" + Hex.ToHexString(bytes[1..])),
                0x05 => new EraInfoKey("era-" + BitConverter.ToInt64(bytes, 1)),
                0x06 => new BalanceKey("balance-" + Hex.ToHexString(bytes[1..])),
                0x07 => new BidKey("bid-" + Hex.ToHexString(bytes[1..])),
                0x08 => new WithdrawKey("withdraw-" + Hex.ToHexString(bytes[1..])),
                0x09 => new DictionaryKey("dictionary-" + Hex.ToHexString(bytes[1..])),
                _ => throw new Exception($"Unknown key identifier '{bytes[0]}'")
            };
        }

        public override string ToString()
        {
            return Key;
        }

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
                    JsonSerializerOptions options) =>
                    GlobalStateKey.FromString(reader.GetString());

                public override void Write(
                    Utf8JsonWriter writer,
                    GlobalStateKey value,
                    JsonSerializerOptions options) =>
                    writer.WriteStringValue(value.ToString());
            }
        }
    }

    public class AccountHashKey : GlobalStateKey
    {
        public AccountHashKey(string key) : base(key, "account-hash-")
        {
            KeyIdentifier = KeyIdentifier.Account;
        }

        public AccountHashKey(PublicKey publicKey)
            : base("account-hash-" + Hex.ToHexString(publicKey.GetAccountHash()), "account-hash-")
        {
        }
    }

    public class HashKey : GlobalStateKey
    {
        public HashKey(string key) : base(key, "hash-")
        {
            KeyIdentifier = KeyIdentifier.Hash;
        }
    }

    public class TransferKey : GlobalStateKey
    {
        public TransferKey(string key) : base(key, "transfer-")
        {
            KeyIdentifier = KeyIdentifier.Transfer;
        }
    }

    public class DeployInfoKey : GlobalStateKey
    {
        public DeployInfoKey(string key) : base(key, "deploy-")
        {
            KeyIdentifier = KeyIdentifier.DeployInfo;
        }
    }

    public class EraInfoKey : GlobalStateKey
    {
        public EraInfoKey(string key) : base(key, "era-")
        {
            KeyIdentifier = KeyIdentifier.EraInfo;
        }

        protected override byte[] _GetRawBytesFromKey(string key)
        {
            var u64 = ulong.Parse(key.Substring(4));
            byte[] bytes = BitConverter.GetBytes(u64);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return bytes;
        }
    }

    public class BalanceKey : GlobalStateKey
    {
        public BalanceKey(string key) : base(key, "balance-")
        {
            KeyIdentifier = KeyIdentifier.Balance;
        }
    }

    public class BidKey : GlobalStateKey
    {
        public BidKey(string key) : base(key, "bid-")
        {
            KeyIdentifier = KeyIdentifier.Bid;
        }
    }

    public class WithdrawKey : GlobalStateKey
    {
        public WithdrawKey(string key) : base(key, "withdraw-")
        {
            KeyIdentifier = KeyIdentifier.Withdraw;
        }
    }

    public class DictionaryKey : GlobalStateKey
    {
        public DictionaryKey(string key) : base(key, "dictionary-")
        {
            KeyIdentifier = KeyIdentifier.Dictionary;
        }
    }
}
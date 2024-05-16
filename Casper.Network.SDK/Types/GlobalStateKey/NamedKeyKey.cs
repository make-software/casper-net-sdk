using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class NamedKeyKey : GlobalStateKey
    {
        private const string NAMEDKEYPREFIX = "named-key-";

        public AddressableEntityKey AddressableEntity { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(NAMEDKEYPREFIX+EntityKind.System.ToKeyPrefix()))
                return NAMEDKEYPREFIX+EntityKind.System.ToKeyPrefix();
            if (key.StartsWith(NAMEDKEYPREFIX+EntityKind.Account.ToKeyPrefix()))
                return NAMEDKEYPREFIX+EntityKind.Account.ToKeyPrefix();
            if (key.StartsWith(NAMEDKEYPREFIX+EntityKind.Contract.ToKeyPrefix()))
                return NAMEDKEYPREFIX+EntityKind.Contract.ToKeyPrefix();

            throw new Exception("Unexpected key prefix in NamedKeyKey: " + key);
        }
        
        public NamedKeyKey(string key) : base(key, NamedKeyKey.GetPrefix(key))
        {
            KeyIdentifier = KeyIdentifier.NamedKey;

            var subkey = key.Substring(NAMEDKEYPREFIX.Length);
            subkey = subkey.Remove(subkey.LastIndexOf('-'));
            AddressableEntity = new AddressableEntityKey(subkey);
        }

        private static string _getString(byte[] key)
        {
            var entityAddress = new AddressableEntityKey(key.Slice(0, 33));
            var addr = Hex.ToHexString(key.Slice(33));
            return $"{NAMEDKEYPREFIX}{entityAddress}-{addr}";
        }
        
        public NamedKeyKey(byte[] key) : this(_getString(key))
        {
        }
    }
}
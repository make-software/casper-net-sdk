using System;
using System.IO;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum EntityKind {
        /// <summary>
        /// System variant.
        /// </summary>
        System = 0,
        /// <summary>
        /// Account variant.
        /// </summary>
        Account = 1,
        /// <summary>
        /// SmartContract variant.
        /// </summary>
        Contract = 2,
    }
    
    public static class EntityKindExtensions
    {
        public static string ToKeyPrefix(this EntityKind kind)
        {
            switch (kind)
            {
                case EntityKind.System:
                    return "entity-system-";
                case EntityKind.Account:
                    return "entity-account-";
                case EntityKind.Contract:
                    return "entity-contract-";
                default:
                    return kind.ToString();
            }
        }
    }
    
    public class AddressableEntityKey : GlobalStateKey
    {
        public EntityKind Kind { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(EntityKind.System.ToKeyPrefix()))
                return EntityKind.System.ToKeyPrefix();
            if (key.StartsWith(EntityKind.Account.ToKeyPrefix()))
                return EntityKind.Account.ToKeyPrefix();
            if (key.StartsWith(EntityKind.Contract.ToKeyPrefix()))
                return EntityKind.Contract.ToKeyPrefix();

            throw new Exception("Unexpected key prefix in NamedKeyKey: " + key);
            
        }
        public AddressableEntityKey(string key) : base(key, GetPrefix(key))
        {
            KeyIdentifier = KeyIdentifier.AddressableEntity;
            var prefix = GetPrefix(key);
            if (EntityKind.System.ToKeyPrefix().Equals(prefix))
                Kind = EntityKind.System;
            else if (EntityKind.Account.ToKeyPrefix().Equals(prefix))
                Kind = EntityKind.Account;
            else if (EntityKind.Contract.ToKeyPrefix().Equals(prefix))
                Kind = EntityKind.Contract;
        }

        public AddressableEntityKey(byte[] key) : this( 
            key[0] == (byte)EntityKind.System
                ? EntityKind.System.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                : (key[0] == (byte)EntityKind.Account
                    ? EntityKind.Account.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                    : (key[0] == (byte)EntityKind.Account
                        ? EntityKind.Contract.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                        : throw new Exception($"Wrong entity tag '{key[0]}' for AddressableEntityKey."))))
        {
        }

        public AddressableEntityKey(BinaryReader reader) : base(null)
        {
            KeyIdentifier = KeyIdentifier.AddressableEntity;
            var tag = reader.ReadByte();
            var addr = reader.ReadBytes(32);
            Kind = (EntityKind)tag;
            Key = Kind.ToKeyPrefix() + Hex.ToHexString(addr);
        }
    }
}
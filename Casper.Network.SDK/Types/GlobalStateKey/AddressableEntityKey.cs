using System;
using System.Collections.Generic;
using System.IO;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum EntityKindEnum {
        /// <summary>
        /// Package associated with a native contract implementation.
        /// </summary>
        System = 0,
        /// <summary>
        /// Package associated with an Account hash.
        /// </summary>
        Account = 1,
        /// <summary>
        /// Package associated with Wasm stored on chain.
        /// </summary>
        Contract = 2,
    }
    
    public static class EntityKinEnumExtensions
    {
        public static string ToKeyPrefix(this EntityKindEnum kind)
        {
            switch (kind)
            {
                case EntityKindEnum.System:
                    return "entity-system-";
                case EntityKindEnum.Account:
                    return "entity-account-";
                case EntityKindEnum.Contract:
                    return "entity-contract-";
                default:
                    return kind.ToString();
            }
        }
    }
    
    public class AddressableEntityKey : GlobalStateKey, IEntityIdentifier, IPurseIdentifier
    {
        public EntityKindEnum Kind { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(EntityKindEnum.System.ToKeyPrefix()))
                return EntityKindEnum.System.ToKeyPrefix();
            if (key.StartsWith(EntityKindEnum.Account.ToKeyPrefix()))
                return EntityKindEnum.Account.ToKeyPrefix();
            if (key.StartsWith(EntityKindEnum.Contract.ToKeyPrefix()))
                return EntityKindEnum.Contract.ToKeyPrefix();

            throw new Exception("Unexpected key prefix in NamedKeyKey: " + key);
            
        }
        public AddressableEntityKey(string key) : base(key, GetPrefix(key))
        {
            KeyIdentifier = KeyIdentifier.AddressableEntity;
            var prefix = GetPrefix(key);
            if (EntityKindEnum.System.ToKeyPrefix().Equals(prefix))
                Kind = EntityKindEnum.System;
            else if (EntityKindEnum.Account.ToKeyPrefix().Equals(prefix))
                Kind = EntityKindEnum.Account;
            else if (EntityKindEnum.Contract.ToKeyPrefix().Equals(prefix))
                Kind = EntityKindEnum.Contract;
        }

        public AddressableEntityKey(byte[] key) : this( 
            key[0] == (byte)EntityKindEnum.System
                ? EntityKindEnum.System.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                : (key[0] == (byte)EntityKindEnum.Account
                    ? EntityKindEnum.Account.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                    : (key[0] == (byte)EntityKindEnum.Account
                        ? EntityKindEnum.Contract.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                        : throw new Exception($"Wrong entity tag '{key[0]}' for AddressableEntityKey."))))
        {
        }

        public AddressableEntityKey(BinaryReader reader) : base(null)
        {
            KeyIdentifier = KeyIdentifier.AddressableEntity;
            var tag = reader.ReadByte();
            var addr = reader.ReadBytes(32);
            Kind = (EntityKindEnum)tag;
            Key = Kind.ToKeyPrefix() + Hex.ToHexString(addr);
        }
        
        protected override byte[] _GetRawBytesFromKey(string key)
        {
            return this.GetBytes();
        }
        
        public override byte[] GetBytes()
        {
            var key = Key.Substring(Key.LastIndexOf('-')+1);
            var rawBytes = Hex.Decode(key);
            var ms = new MemoryStream();
            ms.WriteByte((byte)this.Kind);
            ms.Write(rawBytes);

            return ms.ToArray();
        }
        
        /// <summary>
        /// Returns an EntityIdentifier object as defined in the RPC schema for an account hash key.
        /// </summary>
        public Dictionary<string, object> GetEntityIdentifier()
        {
            return new Dictionary<string, object>
            {
                {"EntityAddr", Key}
            };
        }
        
        /// <summary>
        /// Returns a PurseIdentifier object as defined in the RPC schema for an entity address
        /// </summary>
        public Dictionary<string, object> GetPurseIdentifier()
        {
            return new Dictionary<string, object>
            {
                {"main_purse_under_entity_addr", this.ToString()}
            };
        }
    }
}

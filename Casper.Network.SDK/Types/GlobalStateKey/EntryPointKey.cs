using System;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public class EntryPointKey : GlobalStateKey
    {
        public static string ENTRYPOINT_PREFIX = "entry-point-";
        public static string V1_PREFIX = "v1-";
        public static string V2_PREFIX = "v2-";

        public AddressableEntityKey AddressableEntity { get; init; }

        public string NameHash { get; init; }

        public UInt32 Separator { get; init; }

        public EntryPointKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.EntryPoint;

            if (!key.StartsWith(ENTRYPOINT_PREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{ENTRYPOINT_PREFIX}'.");
            key = key.Substring(ENTRYPOINT_PREFIX.Length);

            if (key.StartsWith(V1_PREFIX))
            {
                key = key.Substring(V1_PREFIX.Length);
                var parts = key.Split('-');
                if (parts.Length != 4)
                    throw new Exception("Key not valid. It should have an entity address and a name hash.");

                AddressableEntity = new AddressableEntityKey($"{parts[0]}-{parts[1]}-{parts[2]}");
                NameHash = parts[3];
            }
            else if (key.StartsWith(V2_PREFIX))
            {
                throw new Exception($"entry-point-v2 not yet supported. {key}.");
            }
        }

        public EntryPointKey(byte[] key) : base(null)
        {
            throw new Exception($"entry-point key from bytes not yet supported. {key}.");
        }
    }
}
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public class PackageKey : GlobalStateKey
    {
        public static string KEYPREFIX = "package-";

        public PackageKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.Package;
        }

        public PackageKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }
}
using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A PublicKey, AccountHashKey, AddressableEntityKey or URef object that identifies a purse.
    /// For keys, the main purse under the key is identified automatically.
    /// </summary>
    public interface IPurseIdentifier
    {
        public Dictionary<string, object> GetPurseIdentifier();
    }
}

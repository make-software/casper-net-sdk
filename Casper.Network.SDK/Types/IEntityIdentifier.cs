using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    public interface IEntityIdentifier
    {
        public Dictionary<string, object> GetEntityIdentifier();
    }
}
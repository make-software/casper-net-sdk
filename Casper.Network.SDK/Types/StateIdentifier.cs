using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    public class StateIdentifier
    {
        private string _stateRootHash;
        private string _blockHash;
        private ulong? _blockHeight;

        private StateIdentifier()
        {
        }
        
        public static StateIdentifier WithStateRootHash(string stateRootHash)
        {
            return new StateIdentifier
            {
                _stateRootHash = stateRootHash,
                _blockHash = null,
                _blockHeight = null
            };
        }
        
        public static StateIdentifier WithBlockHash(string blockHash)
        {
            return new StateIdentifier
            {
                _stateRootHash = null,
                _blockHash = blockHash,
                _blockHeight = null
            };
        }
        
        public static StateIdentifier WithBlockHeight(ulong blockHeight)
        {
            return new StateIdentifier
            {
                _stateRootHash = null,
                _blockHash = null,
                _blockHeight = blockHeight
            };
        }

        public Dictionary<string, object> GetParam()
        {
            if (_stateRootHash != null)
                return new Dictionary<string, object> { {"StateRootHash" , _stateRootHash } };
            if (_blockHash != null)
                return new Dictionary<string, object> { {"BlockHash" , _blockHash } };
            if (_blockHeight != null)
                return new Dictionary<string, object> { {"BlockHeight" , _blockHeight } };

            return null;
        }
    }
}

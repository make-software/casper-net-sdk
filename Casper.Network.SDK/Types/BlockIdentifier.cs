using System;
using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A block identified by its hash or its height.
    /// </summary>
    public interface IBlockIdentifier
    {
        public Dictionary<string, object> GetBlockIdentifier();
    }

    /// <summary>
    /// A block identified by its hash or its height.
    /// </summary>
    public class BlockIdentifier : IBlockIdentifier
    {
        private string _hash;
        private UInt64? _height;

        public BlockIdentifier(string hash)
        {
            _hash = hash;
        }
        
        public BlockIdentifier(UInt64 height)
        {
            _height = height;
        }
        
        /// <summary>
        /// Returns a BlockIdentifier object as defined in the RPC schema for a block.
        /// </summary>
        public Dictionary<string, object> GetBlockIdentifier()
        {
            if(_height.HasValue)
                return new Dictionary<string, object>
                {
                    { "Height", _height.Value},
                };
            
            return new Dictionary<string, object>
            {
                { "Hash", _hash},
            };
        }
    }
}
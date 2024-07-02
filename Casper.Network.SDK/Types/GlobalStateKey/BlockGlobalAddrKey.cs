using System;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public enum BlockGlobalAddrTag
    {
        /// <summary>
        /// Tag for block time variant.
        /// </summary>
        Time = 0,
        /// <summary>
        /// Tag for processing variant.
        /// </summary>
        MessageCount = 1,
    }
    
    public static class BlockGlobalAddrTagExtensions
    {
        public static string ToKeyPrefix(this BlockGlobalAddrTag tag)
        {
            switch (tag)
            {
                case BlockGlobalAddrTag.Time:
                    return "block-time-";
                case BlockGlobalAddrTag.MessageCount:
                    return "block-message-count-";
                default:
                    return tag.ToString();
            }
        }
    }
    
    /// <summary>
    /// Address for singleton values associated to specific block. These are values which are
    /// calculated or set during the execution of a block such as the block timestamp, or the
    /// total count of messages emitted during the execution of the block, and so on.
    /// </summary>
    public class BlockGlobalAddrKey : GlobalStateKey
    {
        public BlockGlobalAddrTag Tag { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(BlockGlobalAddrTag.Time.ToKeyPrefix()))
                return BlockGlobalAddrTag.Time.ToKeyPrefix();
            if (key.StartsWith(BlockGlobalAddrTag.MessageCount.ToKeyPrefix()))
                return BlockGlobalAddrTag.MessageCount.ToKeyPrefix();

            throw new Exception("Unexpected key prefix in NamedKeyKey: " + key);
        }
        
        public BlockGlobalAddrKey(string key) : base(key, GetPrefix(key))
        {
            KeyIdentifier = KeyIdentifier.BlockGlobal;
            Tag = GetPrefix(key) == BlockGlobalAddrTag.Time.ToKeyPrefix()
                ? BlockGlobalAddrTag.Time
                : BlockGlobalAddrTag.MessageCount;
        }

        public BlockGlobalAddrKey(byte[] key) : this( 
            key[0] == (byte)BlockGlobalAddrTag.Time
                ? BlockGlobalAddrTag.Time.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                : (key[0] == (byte)BlockGlobalAddrTag.MessageCount
                    ? BlockGlobalAddrTag.MessageCount.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                    :  throw new Exception($"Wrong kind tag '{key[0]}' for BlockGlobalAddrKey.")))
        {
        }
    }
}
using System;
using System.IO;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class MessageKey : GlobalStateKey
    {
        private const string KEY_PREFIX = "message-";
        private const string TOPIC_PREFIX = "topic-";
        
        public string HashAddr { get; init; }
        
        public string TopicHash { get; init; }
        
        public UInt32? Index { get; init; }
        
        public MessageKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.Message;
            
            if (!key.StartsWith(KEY_PREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{KEY_PREFIX}'.");
            key = key.Substring(KEY_PREFIX.Length);

            if (key.StartsWith(TOPIC_PREFIX))
            {
                key = key.Substring(TOPIC_PREFIX.Length);
                key = key.Replace("entity-contract-", "");
                key = key.Replace("entity-system-", "");
                key = key.Replace("entity-account-", "");
                var parts = key.Split('-');
                if(parts.Length == 2)
                {
                    HashAddr = parts[0];
                    TopicHash = parts[1];
                }
                else
                    throw new Exception("Key not valid. It should have a hash address and a topic hash.");
            }
            else
            {
                key = key.Replace("entity-contract-", "");
                key = key.Replace("entity-system-", "");
                key = key.Replace("entity-account-", "");
                var parts = key.Split('-');
                if (parts.Length == 3)
                {
                    HashAddr = parts[0];
                    TopicHash = parts[1];
                    
                    if(parts[2].Length == 0)
                        throw new Exception("Key not valid. Expected a non-empty message index.");
                    Index = Convert.ToUInt32(parts[2], 16);
                }
                else
                    throw new Exception("Key not valid. It should have a hash address, a topic hash, and a message index.");
            }
        }
    
        public MessageKey(byte[] key) : base(null)
        {
            KeyIdentifier = KeyIdentifier.Message;

            var ms = new MemoryStream(key);
            var reader = new BinaryReader(ms);
            
            var hash = reader.ReadBytes(32);
            HashAddr = Hex.ToHexString(hash);
            
            var topic = reader.ReadBytes(32);
            TopicHash = Hex.ToHexString(topic);
            
            var optionIndexTag = reader.ReadByte();
            if (optionIndexTag == 0x01)
                Index = reader.ReadUInt32();

            Key = KEY_PREFIX +
                  (Index.HasValue ? "" : TOPIC_PREFIX) +
                  HashAddr + "-" +
                  TopicHash +
                  (Index.HasValue ? "-" + Index.Value.ToString("x") : "");
        }
    }
}
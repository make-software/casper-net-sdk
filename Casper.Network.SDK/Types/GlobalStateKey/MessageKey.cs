using System;
using System.IO;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class MessageKey : GlobalStateKey
    {
        private const string MESSAGE_PREFIX = "message-";
        private const string TOPIC_PREFIX = "topic-";
        
        private static readonly string CONTRACT_TOPIC_KEYPREFIX = "message-topic-entity-contract-";
        private static readonly string CONTRACT_MSG_KEYPREFIX = "message-entity-contract-";

        public AddressableEntityKey AddressableEntity { get; init; }
        
        public string TopicHash { get; init; }
        
        public UInt32? Index { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(CONTRACT_TOPIC_KEYPREFIX))
                return CONTRACT_TOPIC_KEYPREFIX;
            if (key.StartsWith(CONTRACT_MSG_KEYPREFIX))
                return CONTRACT_MSG_KEYPREFIX;
            
            throw new Exception("Unexpected key prefix in ByteCodeKey: " + key);
        }
        
        public MessageKey(string key) : base(key)
        {
            KeyIdentifier = KeyIdentifier.Message;
            
            if (!key.StartsWith(MESSAGE_PREFIX))
                throw new ArgumentException($"Key not valid. It should start with '{MESSAGE_PREFIX}'.");
            key = key.Substring(MESSAGE_PREFIX.Length);

            if (key.StartsWith(TOPIC_PREFIX))
            {
                key = key.Substring(TOPIC_PREFIX.Length);
                var parts = key.Split('-');
                if (parts.Length != 4)
                    throw new Exception("Key not valid. It should have an entity address and a topic hash.");

                AddressableEntity = new AddressableEntityKey($"{parts[0]}-{parts[1]}-{parts[2]}");
                TopicHash = parts[3];
            }
            else
            {
                var parts = key.Split('-');
                if (parts.Length != 5)
                    throw new Exception("Key not valid. It should have an entity address, a topic hash, and a message index.");

                AddressableEntity = new AddressableEntityKey($"{parts[0]}-{parts[1]}-{parts[2]}");
                TopicHash = parts[3];
                
                if(parts[4].Length == 0)
                    throw new Exception("Key not valid. Expected a non-empty message index.");
                Index = Convert.ToUInt32(parts[4], 16);
            }
        }
    
        public MessageKey(byte[] key) : base(null)
        {
            KeyIdentifier = KeyIdentifier.Message;

            var ms = new MemoryStream(key);
            var reader = new BinaryReader(ms);
            
            AddressableEntity = new AddressableEntityKey(reader);
            
            var topic = reader.ReadBytes(32);
            TopicHash = Hex.ToHexString(topic);
            
            var optionIndexTag = reader.ReadByte();
            if (optionIndexTag == 0x01)
                Index = reader.ReadUInt32();

            Key = MESSAGE_PREFIX +
                  (Index.HasValue ? "" : TOPIC_PREFIX) +
                  AddressableEntity.ToString() + "-" +
                  TopicHash +
                  (Index.HasValue ? "-" + Index.Value.ToString("x") : "");
        }
    }
}
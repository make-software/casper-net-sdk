using System;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Summary of a message topic that will be stored in global state.
    /// </summary>
    public class MessageTopicSummary
    {
        /// <summary>
        /// Block timestamp in which these messages were emitted.
        /// </summary>
        [JsonPropertyName("blocktime")]
        public UInt64 BlockTime { get; init; }
        
        /// <summary>
        /// Number of messages in this topic.
        /// </summary>
        [JsonPropertyName("message_count")]
        public UInt32 MessageCount { get; init; }
    }

    /// <summary>
    /// The payload of a message.
    /// </summary>
    public class MessagePayload
    {
        /// <summary>
        /// Human readable string message.
        /// </summary>
        public string String { get; init; }
        /// <summary>
        /// Message represented as raw bytes.
        /// </summary>
        public string Bytes { get; init; }
    }
    
    /// <summary>
    /// Message that was emitted by an addressable entity during execution.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The identity of the entity that produced the message.
        /// </summary>
        [JsonPropertyName("hash_addr")]
        public string HashAddr { get; init; }
        
        /// <summary>
        /// The payload of the message.
        /// </summary>
        [JsonPropertyName("message")]
        public MessagePayload MessagePayload { get; init; }
        
        /// <summary>
        /// The name of the topic on which the message was emitted on.
        /// </summary>
        [JsonPropertyName("topic_name")]
        public string TopicName { get; init; }
        
        /// <summary>
        /// The hash of the name of the topic.
        /// </summary>
        [JsonPropertyName("topic_name_hash")]
        public string TopicNameHash { get; init; }
        
        /// <summary>
        /// Message index in the topic.
        /// </summary>
        [JsonPropertyName("topic_index")]
        public uint TopicIndex { get; init; }
        
        /// <summary>
        /// Message index in the block.
        /// </summary>
        [JsonPropertyName("block_index")]
        public ulong BlockIndex { get; init; }
    }
}
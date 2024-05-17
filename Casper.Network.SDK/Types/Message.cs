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

    public class MessageChecksum
    {
        
    }
}
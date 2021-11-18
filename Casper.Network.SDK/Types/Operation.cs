using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The type of operation performed while executing a deploy.
    /// </summary>
    public enum OpKind
    {
        Read,
        Write,
        Add,
        NoOp
    }
    
    /// <summary>
    /// An operation performed while executing a deploy.
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// The formatted string of the `Key`.
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; init; }

        /// <summary>
        /// The type of operation.
        /// </summary>
        [JsonPropertyName("kind")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OpKind Kind { get; init; }
    }
}
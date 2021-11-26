using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A named key.
    /// </summary>
    public class NamedKey
    {
        /// <summary>
        /// The name of the entry.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; }

        /// <summary>
        /// The value of the entry: a casper `Key` type.
        /// </summary>
        [JsonPropertyName("key")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public GlobalStateKey Key { get; init; }
    }
}
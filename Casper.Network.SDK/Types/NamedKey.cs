using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A named key in an Account or Contract.
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
    
    /// <summary>
    /// A named key in an addressable entity.
    /// </summary>
    public class NamedKeyValue
    {
        /// <summary>
        /// The name of the entry.
        /// </summary>
        [JsonPropertyName("name")]
        public CLValue Name { get; init; }

        /// <summary>
        /// The value of the entry: a casper `Key` type.
        /// </summary>
        [JsonPropertyName("named_key")]
        public CLValue Key { get; init; }
    }
}
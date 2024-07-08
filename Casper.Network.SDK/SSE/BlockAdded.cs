using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A <see cref="Block">Block</see> that has been added to the linear chain and stored in the node.
    /// </summary>
    public class BlockAdded
    {
        /// <summary>
        /// The <see cref="Block">Block</see> hash.
        /// </summary>
        [JsonPropertyName("block_hash")] 
        public string BlockHash { get; init; }

        /// <summary>
        /// The <see cref="Block">Block</see> data.
        /// </summary>
        [JsonPropertyName("block")]
        [JsonConverter(typeof(BlockConverter))]
        public Block Block { get; init; }
        
        public class BlockConverter : JsonConverter<Block>
        {
            public override Block Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                    {
                        if (document.RootElement.TryGetProperty("header", out var header))
                        {
                            var blockV1 = JsonSerializer.Deserialize<BlockV1>(document.RootElement.GetRawText());
                            return (Block)blockV1;
                        }
                        
                        var blockSerializerOptions = new JsonSerializerOptions(options);
                        blockSerializerOptions.Converters.Add(new Block.BlockConverter());
                        var block = JsonSerializer.Deserialize<Block>(document.RootElement.GetRawText(), blockSerializerOptions);
                        return block;
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                Block block,
                JsonSerializerOptions options)
            {
                throw new JsonException(
                    "Cannot serialize a block with this converter. Use Block.BlockConverter instead");
            }
        }
    }
}
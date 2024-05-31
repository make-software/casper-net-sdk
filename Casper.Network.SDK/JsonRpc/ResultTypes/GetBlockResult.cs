using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    internal class BlockV1Compat
    {
        [JsonPropertyName("hash")]
        public string Hash { get; init; }
        
        [JsonPropertyName("header")]
        public BlockHeaderV1 Header { get; init; }

        [JsonPropertyName("body")]
        public BlockBodyV1 Body { get; init; }
        
        [JsonPropertyName("proofs")]
        public List<BlockProof> Proofs { get; init; }
    }
    
    /// <summary>
    /// A JSON-friendly representation of a block and the signatures for that block
    /// </summary>
    internal class BlockWithSignatures
    {
        /// <summary>
        /// The block.
        /// </summary>
        [JsonPropertyName("block")]
        [JsonConverter(typeof(Block.BlockConverter))]
        public IBlock Block { get; init; }

        /// <summary>
        /// The proofs of the block, i.e. a collection of validators' signatures of the block hash.
        /// </summary>
        [JsonPropertyName("proofs")]
        public List<BlockProof> Proofs { get; init; }
    }
    
    internal class GetBlockResultCompat : RpcResult
    {
        /// <summary>
        /// The block, if found.
        /// </summary>
        [JsonPropertyName("block_with_signatures")]
        public BlockWithSignatures BlockWithSignatures { get; init; }
        
        [JsonPropertyName("block")]
        public BlockV1Compat BlockV1 { get; init; }
    }
    
    /// <summary>
    /// Result for "chain_get_block" RPC response.
    /// </summary>
    [JsonConverter(typeof(GetBlockResultConverter))]
    public class GetBlockResult : RpcResult
    {
        /// <summary>
        /// The block.
        /// </summary>
        [JsonPropertyName("block")]
        [JsonConverter(typeof(Block.BlockConverter))]
        public IBlock Block { get; init; }

        /// <summary>
        /// The proofs of the block, i.e. a collection of validators' signatures of the block hash.
        /// </summary>
        [JsonPropertyName("proofs")]
        public List<BlockProof> Proofs { get; init; }
        
        public class GetBlockResultConverter : JsonConverter<GetBlockResult>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(GetBlockResult);
            }

            public override GetBlockResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var resultCompat = JsonSerializer.Deserialize<GetBlockResultCompat>(ref reader, options);

                    if (resultCompat.BlockV1 != null)
                    {
                        return new GetBlockResult()
                        {
                            ApiVersion = resultCompat.ApiVersion,
                            Block = new BlockV1()
                            {
                                Hash = resultCompat.BlockV1.Hash,
                                Header = resultCompat.BlockV1.Header,
                                Body = resultCompat.BlockV1.Body,
                            },
                            Proofs = resultCompat.BlockV1.Proofs,
                        };
                    }
                    return new GetBlockResult()
                    {
                        ApiVersion = resultCompat.ApiVersion,
                        Block = resultCompat.BlockWithSignatures.Block,
                        Proofs = resultCompat.BlockWithSignatures.Proofs,
                    };
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                GetBlockResult block,
                JsonSerializerOptions options)
            {
                throw new JsonException($"not yet implemented");
            }
        }
    }
}

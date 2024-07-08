using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A validator's signature of a block, confirming it is finalized. Produced in Casper v1.x
    /// </summary>
    public class FinalitySignatureV1
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }

        /// <summary>
        /// The block era id.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// Validator public key
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Validator signature
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }


    internal class FinalitySignatureV2 : FinalitySignatureV1
    {
        /// <summary>
        /// The block height
        /// </summary>
        [JsonPropertyName("block_height")]
        public ulong BlockHeight { get; init; }
        
        /// <summary>
        /// The hash of the chain name of the associated block. 
        /// </summary>
        [JsonPropertyName("chain_name_hash")]
        public string ChainNameHash { get; init; }
    }
    
    internal class FinalitySignatureCompat
    {
        [JsonPropertyName("V1")]
        public FinalitySignatureV1 Version1 { get; init; }
        
        [JsonPropertyName("V2")]
        public FinalitySignatureV2 Version2 { get; init; }
        
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }

        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
    }
    
    /// <summary>
    /// A validator's signature of a block, confirming it is finalized.
    /// </summary>
    [JsonConverter(typeof(FinalitySignatureConverter))]
    public class FinalitySignature
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the finality signature.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        protected FinalitySignatureV1 _finalitySignatureV1;

        public static explicit operator FinalitySignatureV1(FinalitySignature finalitySignature)
        {
            if(finalitySignature._version == 1)
                return finalitySignature._finalitySignatureV1;

            throw new InvalidCastException("Version2 FinalitySignature cannot be converted to Version1");
        }
        
        public static explicit operator FinalitySignature(FinalitySignatureV1 finalitySignature)
        {
            return new FinalitySignature
            {
                _version = 1,
                _finalitySignatureV1 = finalitySignature,
                BlockHash = finalitySignature.BlockHash,
                BlockHeight = 0, 
                EraId = finalitySignature.EraId,
                PublicKey= finalitySignature.PublicKey,
                Signature = finalitySignature.Signature,
                ChainNameHash = null,
            };
        }
        
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; } 
       
       /// <summary>
        /// The block height
        /// </summary>
        [JsonPropertyName("block_height")]
        public ulong BlockHeight { get; init; }
        
        /// <summary>
        /// The hash of the chain name of the associated block. 
        /// </summary>
        [JsonPropertyName("chain_name_hash")]
        public string ChainNameHash { get; init; }

        /// <summary>
        /// The block era id.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }

        /// <summary>
        /// Validator public key
        /// </summary>
        [JsonPropertyName("public_key")]
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Validator signature
        /// </summary>
        [JsonPropertyName("signature")]
        [JsonConverter(typeof(Signature.SignatureConverter))]
        public Signature Signature { get; init; }
        
        /// <summary>
        /// Json converter class to serialize/deserialize a FinalitySignature to/from Json
        /// </summary>
        public class FinalitySignatureConverter : JsonConverter<FinalitySignature>
        {
            public override FinalitySignature Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var fsCompat = JsonSerializer.Deserialize<FinalitySignatureCompat>(ref reader, options);
                    if (fsCompat.BlockHash != null)
                    {
                        var v1 = new FinalitySignatureV1()
                        {
                            BlockHash = fsCompat.BlockHash,
                            EraId = fsCompat.EraId,
                            PublicKey = fsCompat.PublicKey,
                            Signature = fsCompat.Signature,

                        };
                        return (FinalitySignature)v1;
                    }
                    if (fsCompat.Version1 != null)
                    {
                        return (FinalitySignature)fsCompat.Version1;
                    }
                    if (fsCompat.Version2 != null)
                    {
                        return new FinalitySignature
                        {
                            _version = 2,
                            BlockHash = fsCompat.Version2.BlockHash,
                            BlockHeight = fsCompat.Version2.BlockHeight,
                            EraId = fsCompat.Version2.EraId,
                            PublicKey = fsCompat.Version2.PublicKey,
                            Signature = fsCompat.Version2.Signature,
                            ChainNameHash = fsCompat.Version2.ChainNameHash,
                        };
                    }

                    throw new JsonException("Cannot deserialize FinalitySignature");
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                FinalitySignature finalitySignature,
                JsonSerializerOptions options)
            {
                switch (finalitySignature.Version)
                {
                    case 1:
                        writer.WritePropertyName("V1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((FinalitySignatureV1)finalitySignature, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("V2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(finalitySignature, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected finality signature version {finalitySignature.Version}");
                }
            }
        }
    }
}

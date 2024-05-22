using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// A validator's signature of a block, confirming it is finalized. Produced in Casper v1.x
    /// </summary>
    public class FinalitySignatureV1 : FinalitySignature
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

    /// <summary>
    /// A validator's signature of a block, confirming it is finalized. Produced in Casper v2.x
    /// </summary>
    public class FinalitySignatureV2 : FinalitySignatureV1
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
    
    [JsonConverter(typeof(FinalitySignature.FinalitySignatureConverter))]
    public interface IFinalitySignature
    {
        public int Version { get; }
        
        public FinalitySignatureV1 FinalitySignatureV1 { get; }
        
        public FinalitySignatureV2 FinalitySignatureV2 { get; }
    }
    
    /// <summary>
    /// A validator's signature of a block, confirming it is finalized.
    /// </summary>
    public class FinalitySignature : IFinalitySignature
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the transfer.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        /// <summary>
        /// Returns the transfer as a Version1 transfer object.
        /// </summary>
        FinalitySignatureV1 IFinalitySignature.FinalitySignatureV1 => this as FinalitySignatureV1;

        /// <summary>
        /// Returns the transfer as a Version2 transfer object.
        /// </summary>
        FinalitySignatureV2 IFinalitySignature.FinalitySignatureV2 => this as FinalitySignatureV2;

        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class FinalitySignatureConverter : JsonConverter<IFinalitySignature>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(IFinalitySignature) ||
                       typeToConvert == typeof(FinalitySignature);
            }

            public override IFinalitySignature Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    reader.Read();
                    var version = reader.GetString();
                    reader.Read();
                    switch (version)
                    {
                        case "V1":
                        {
                            var finalitySignature1 = JsonSerializer.Deserialize<FinalitySignatureV1>(ref reader, options);
                            reader.Read();
                            finalitySignature1._version = 1;
                            return finalitySignature1;
                        }
                        case "V2":
                            var finalitySignature2 = JsonSerializer.Deserialize<FinalitySignatureV2>(ref reader, options);
                            reader.Read();
                            finalitySignature2._version = 2;
                            return finalitySignature2;
                        default:
                            throw new JsonException("Expected V1 or V2");
                    }

                    ;
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                IFinalitySignature finalitySignature,
                JsonSerializerOptions options)
            {
                switch (finalitySignature.Version)
                {
                    case 1:
                        writer.WritePropertyName("V1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(finalitySignature as FinalitySignatureV1, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("V2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(finalitySignature as FinalitySignatureV2, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected finality signature version {finalitySignature.Version}");
                }
            }
        }
    }
}
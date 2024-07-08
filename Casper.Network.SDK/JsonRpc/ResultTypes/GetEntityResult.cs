using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for \"state_get_entity\" RPC response.
    /// </summary>
    [JsonConverter(typeof(GetEntityResultConverter))]
    public class GetEntityResult : RpcResult
    {
        /// <summary>
        /// An addressable entity.
        /// </summary>
        public AddressableEntity Entity { get; init; }

        /// <summary>
        /// Array of named keys present in the entity.
        /// </summary>
        public List<NamedKey> NamedKeys { get; init; }
        
        /// <summary>
        /// Array of entry points defined in the entity
        /// </summary>
        public List<VersionedEntryPoint> EntryPoints { get; init; }
        
        /// <summary>
        /// A legacy account.
        /// </summary>
        public Account LegacyAccount { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        public string MerkleProof { get; init; }

        public class GetEntityResultConverter : JsonConverter<GetEntityResult>
        {
            public override GetEntityResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                string api_version = null;
                AddressableEntity entity = null;
                var namedKeys = new List<NamedKey>();
                var entryPoints = new List<VersionedEntryPoint>();
                Account legacyAccount = null;
                string merkle_proof = null;
                uint skippedEntityWrapperCount = 0;
                reader.Read();
                
                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();
                    switch (property)
                    {
                        case "api_version":
                            api_version = reader.GetString();
                            break;
                        case "entity":
                            // parse only if we already read AddressableEntity property. Otherwise, this is a wrapper for an AnddressableEntity or a LegacyAccount
                            if (++skippedEntityWrapperCount >= 2)
                                entity = JsonSerializer.Deserialize<AddressableEntity>(ref reader, options);
                            break;
                        case "AddressableEntity":
                            // continue, this is a wrapper for entity (the actual AddressableEntity), named_keys and entry_points.
                            ++skippedEntityWrapperCount;
                            break;
                        case "named_keys":
                            namedKeys = JsonSerializer.Deserialize<List<NamedKey>>(ref reader, options);
                            break;
                        case "entry_points":
                            entryPoints = JsonSerializer.Deserialize<List<VersionedEntryPoint>>(ref reader, options);
                            break;
                        case "LegacyAccount":
                            if (reader.TokenType != JsonTokenType.Null)
                                legacyAccount = JsonSerializer.Deserialize<Account>(ref reader, options);
                            reader.Read();
                            break;
                        case "merkle_proof":
                            merkle_proof = reader.GetString();
                            break;
                    }

                    reader.Read();
                    
                    while (reader.TokenType == JsonTokenType.EndObject && --skippedEntityWrapperCount > 0)
                        reader.Read(); // skip entity wrapper EndObject token and continue parsing GetEntityResult properties
                }

                reader.Read(); // skip outer type EndObject token

                if(entity == null && legacyAccount == null)
                    throw new JsonException($"Could not deserialize GetEntityResult.");

                return new GetEntityResult()
                {
                    ApiVersion = api_version,
                    Entity = entity,
                    NamedKeys = namedKeys,
                    EntryPoints = entryPoints,
                    LegacyAccount = legacyAccount,
                    MerkleProof = merkle_proof
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                GetEntityResult value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for GetEntityResult not yet implemented");
            }
        }
    }
}
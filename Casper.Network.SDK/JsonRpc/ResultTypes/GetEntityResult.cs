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
                Account legacy_account = null;
                string merkle_proof = null;

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
                            if (reader.TokenType == JsonTokenType.StartObject)
                                reader.Read();
                            if (reader.TokenType == JsonTokenType.PropertyName)
                            {
                                property = reader.GetString();
                                reader.Read();
                                switch (property)
                                {
                                    case "AddressableEntity":
                                        if (reader.TokenType != JsonTokenType.Null)
                                            entity = JsonSerializer.Deserialize<AddressableEntity>(ref reader, options);
                                        reader.Read();
                                        break;
                                    case "LegacyAccount":
                                        if (reader.TokenType != JsonTokenType.Null)
                                            legacy_account = JsonSerializer.Deserialize<Account>(ref reader, options);
                                        reader.Read();
                                        break;
                                }
                            }

                            break;
                        case "merkle_proof":
                            merkle_proof = reader.GetString();
                            break;
                    }

                    reader.Read();
                }

                if(entity == null && legacy_account == null)
                    throw new JsonException($"Could not deserialize GetEntityResult..");

                return new GetEntityResult()
                {
                    ApiVersion = api_version,
                    Entity = entity,
                    LegacyAccount = legacy_account,
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
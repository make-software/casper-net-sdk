using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_balance" RPC response.
    /// </summary>
    [JsonConverter(typeof(BalanceResultConverter))]
    public class GetBalanceResult : RpcResult
    {
        /// <summary>
        /// The balance value.
        /// </summary>
        [JsonPropertyName("balance_value")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger BalanceValue { get; init; }

        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }

    public class BalanceResultConverter : JsonConverter<GetBalanceResult>
    {
        public override GetBalanceResult Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string api_version = null;
            BigInteger balance = default;
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
                        reader.Read();
                        break;
                    case "balance":
                    case "balance_value":
                        balance = BigInteger.Parse(reader.GetString());
                        reader.Read();
                        break;
                    case "merkle_proof":
                        merkle_proof = reader.GetString();
                        reader.Read();
                        break;
                    default:
                        throw new JsonException($"Unexpected property '{property}' deserializing QueryBalanceResult");
                }
            }

            reader.Read();

            return new GetBalanceResult
            {
                ApiVersion = api_version,
                BalanceValue = balance,
                MerkleProof = merkle_proof,
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            GetBalanceResult ttlInMillis,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

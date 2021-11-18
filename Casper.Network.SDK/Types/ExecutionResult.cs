using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The result of executing a single deploy.
    /// </summary>
    public class ExecutionResult
    {
        [JsonIgnore] 
        public bool IsSuccess { get; init; }

        /// <summary>
        /// The block hash.
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The cost of executing the deploy.
        /// </summary>
        [JsonPropertyName("cost")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Cost { get; init; }

        /// <summary>
        /// The effect of executing the deploy.
        /// </summary>
        [JsonPropertyName("effect")]
        public ExecutionEffect Effect { get; init; }

        [JsonPropertyName("error_message")] 
        public string ErrorMessage { get; init; }

        /// <summary>
        /// A record of Transfers performed while executing the deploy.
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("transfers")]
        public List<string> Transfers { get; init; }

        public ExecutionResult()
        {
            Transfers = new List<string>();
        }
        
        public class ExecutionResultConverter : JsonConverter<ExecutionResult>
        {
            public override ExecutionResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                    reader.Read();

                string blockHash = null;
                string result = null;
                ExecutionResult executionResult = null;
                
                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();

                    switch (property.ToLower())
                    {
                        case "block_hash":
                            blockHash = reader.GetString();
                            reader.Read();
                            break;
                        case "result":
                            reader.Read(); //skip start object
                            
                            result = (reader.TokenType == JsonTokenType.PropertyName) ? reader.GetString() : null;
                            reader.Read();
                            
                            if (result == null || !(result.Equals("Success") || result.Equals("Failure")))
                                throw new JsonException("Either 'Success' or 'Failure' expect during ExecutionResult deserialization");

                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException($"Object expected after '{result}' during ExecutionResult deserialization");

                            executionResult = JsonSerializer.Deserialize<ExecutionResult>(ref reader);

                            reader.Read(); // end Success/Failure object
                            reader.Read(); // end Result object

                            break;
                    }
                }

                if (executionResult == null)
                    throw new JsonException("Could not deserialize ExecutionResult object");

                return new ExecutionResult
                {
                    BlockHash = blockHash,
                    IsSuccess = result.Equals("Success"),
                    Cost = executionResult.Cost,
                    Effect = executionResult.Effect,
                    ErrorMessage = executionResult.ErrorMessage,
                    Transfers = executionResult.Transfers
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                ExecutionResult item,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Serialization of ExecutionResult not implemented");
            }
        }
    }
}
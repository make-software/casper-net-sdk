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
    public class ExecutionResultV1
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
        [JsonConverter(typeof(GenericListConverter<TransferKey, GlobalStateKey.GlobalStateKeyConverter>))]
        public List<TransferKey> Transfers { get; init; }

        public ExecutionResultV1()
        {
            Transfers = new List<TransferKey>();
        }
        
        public class ExecutionResultConverter : JsonConverter<ExecutionResultV1>
        {
            public override ExecutionResultV1 Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                    reader.Read();

                string blockHash = null;
                string result = null;
                bool readResult = false;
                ExecutionResultV1 executionResult = null;
                
                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();

                    switch (property.ToLowerInvariant())
                    {
                        case "block_hash":
                            blockHash = reader.GetString();
                            reader.Read();
                            break;
                        case "result":
                            readResult = true;
                            reader.Read(); //skip start object
                            break;
                        case "success": 
                        case "failure":
                            result = property;

                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException($"Object expected after '{result}' during ExecutionResult deserialization");

                            executionResult = JsonSerializer.Deserialize<ExecutionResultV1>(ref reader);

                            reader.Read(); // end Success/Failure object
                            
                            // skip the EndObject for "result" json object if it was read only
                            // DeployProcessed.ExecutionResult does not read it
                            if(readResult)
                                reader.Read(); 

                            break;
                    }
                }

                if (executionResult == null)
                    throw new JsonException("Could not deserialize ExecutionResult object");

                return new ExecutionResultV1
                {
                    BlockHash = blockHash,
                    IsSuccess = result.Equals("Success", StringComparison.InvariantCultureIgnoreCase),
                    Cost = executionResult.Cost,
                    Effect = executionResult.Effect,
                    ErrorMessage = executionResult.ErrorMessage,
                    Transfers = executionResult.Transfers
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                ExecutionResultV1 item,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Serialization of ExecutionResult not implemented");
            }
        }
    }

    public class ExecutionResultV2
    {
        /// <summary>
        /// What was the maximum allowed gas limit for this transaction?.
        /// </summary>
        [JsonPropertyName("limit")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Limit { get; init; }

        /// <summary>
        /// How much gas was consumed executing this transaction.
        /// </summary>
        [JsonPropertyName("consumed")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Consumed { get; init; }

        /// <summary>
        /// How much was paid for this transaction.
        /// </summary>
        [JsonPropertyName("cost")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Cost { get; init; }
        
        /// <summary>
        /// If there is no error message, this execution was processed successfully. If there is an error message, this
        /// execution failed to fully process for the stated reason.
        /// </summary>
        [JsonPropertyName("error_message")] 
        public string ErrorMessage { get; init; }
        
        /// <summary>
        /// A record of transfers performed while executing this transaction.
        /// </summary>
        [JsonPropertyName("transfers")] 
        public List<Transfer> Transfers { get; init; }
        
        /// <summary>
        /// The size estimate of the transaction
        /// </summary>
        [JsonPropertyName("size_estimate")] 
        public UInt64 SizeEstimate { get; init; }
        
        /// <summary>
        /// A log of all transforms produced during execution.
        /// </summary>
        [JsonPropertyName("effects")]
        [JsonConverter(typeof(GenericListConverter<TransformV2, TransformV2.TransformV2Converter>))]
        public List<TransformV2> Effect { get; init; }
    }
    
    public class ExecutionResult
    {
        /// <summary>
        /// Version 1 of execution result type.
        /// </summary>
        public ExecutionResultV1 Version1 { get; init; }        
        
        /// <summary>
        /// Version 2 of execution result type.
        /// </summary>
        public ExecutionResultV2 Version2 { get; init; }        
    }
}
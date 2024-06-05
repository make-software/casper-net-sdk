using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    public class ExecutionResult
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the block.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        protected ExecutionResultV1 _executionResultV1;
        
        public static explicit operator ExecutionResultV1(ExecutionResult executionResult)
        {
            if(executionResult._version == 1)
                return executionResult._executionResultV1;

            throw new InvalidCastException("Version2 execution result cannot be converted to Version1");
        }
        
        public static explicit operator ExecutionResult(ExecutionResultV1 executionResult)
        {
            var v2Transfers = executionResult.Transfers.Select(key =>
            {
                var transform = executionResult.Effect.Transforms.FirstOrDefault(tr => tr.Key.Equals(key));
                if (transform != null && 
                    transform.Kind == TransformKindV1.WriteTransfer && 
                    transform.Value is TransferV1 transferV1)
                {
                    return (Transfer)transferV1;
                }
                return null;
            }).ToList();
            
            var v2Effect = executionResult.Effect.Transforms.Select(t => (Transform)t).ToList();
            
            return new ExecutionResult
            {
                _version = 1,
                _executionResultV1 = executionResult,
                ErrorMessage = executionResult.ErrorMessage,
                // Transfers = executionResult.Transfers,
                Cost = executionResult.Cost,
                Limit = 0,
                Consumed = 0,
                Effect = v2Effect,
                SizeEstimate = 0,
                Transfers = v2Transfers,
            };
        }
        
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
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Effect { get; init; }
        
        /// <summary>
        /// Json converter class to serialize/deserialize a ExecutionResult to/from Json
        /// </summary>
        public class ExecutionResultConverter : JsonConverter<ExecutionResult>
        {
            public override ExecutionResult Read(
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
                        case "Version1":
                            var erV1 = JsonSerializer.Deserialize<ExecutionResultV1>(ref reader, options);
                            reader.Read();
                            return (ExecutionResult)erV1;
                        case "Version2":
                            var erv2 = JsonSerializer.Deserialize<ExecutionResultV2>(ref reader, options);
                            reader.Read();
                            return new ExecutionResult
                            {
                                _version = 2,
                                ErrorMessage = erv2.ErrorMessage,
                                Transfers = erv2.Transfers,
                                Cost = erv2.Cost,
                                Limit = erv2.Limit,
                                Consumed = erv2.Consumed,
                                Effect = erv2.Effect,
                                SizeEstimate = erv2.SizeEstimate,
                            };
                        default:
                            throw new JsonException("Expected Version1 or Version2");
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                ExecutionResult executionResult,
                JsonSerializerOptions options)
            {
                switch (executionResult.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((ExecutionResultV1)executionResult, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(executionResult, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected execution result version {executionResult.Version}");
                }
            }
        }
    }
    
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
        
        public class ExecutionResultV1Converter : JsonConverter<ExecutionResultV1>
        {
            public override ExecutionResultV1 Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var firstTokenType = reader.TokenType;
                
                if(firstTokenType == JsonTokenType.StartArray) //skip start array (backward compat)
                    reader.Read();
                    
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
                
                if(firstTokenType == JsonTokenType.StartArray)
                    reader.Read(); // skip end object

                // if (reader.TokenType == JsonTokenType.EndArray)
                    // reader.Read(); //skip end array (backward compat)

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
    
    internal class ExecutionResultV2
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
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Effect { get; init; }
    }
}

using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Represents a transfer from one purse to another
    /// </summary>
    public class TransferV1
    {
        /// <summary>
        /// Transfer amount
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// Deploy that created the transfer
        /// </summary>
        [JsonPropertyName("deploy_hash")]
        public string DeployHash { get; init; }
        
        /// <summary>
        /// Account hash from which transfer was executed
        /// </summary>
        [JsonPropertyName("from")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey From { get; init; }
        
        /// <summary>
        /// Gas
        /// </summary>
        [JsonPropertyName("gas")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Gas { get; init; }
        
        /// <summary>
        /// User-defined id
        /// </summary>
        [JsonPropertyName("id")]
        public ulong? Id { get; init; }
        
        /// <summary>
        /// Source purse
        /// </summary>
        [JsonPropertyName("source")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef Source { get; init; }
        
        /// <summary>
        /// Target purse
        /// </summary>
        [JsonPropertyName("target")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef Target { get; init; }
        
        /// <summary>
        /// Account to which funds are transferred
        /// </summary>
        [JsonPropertyName("to")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey To { get; init; }
    }
    
    /// <summary>
    /// Represents a version 2 transfer from one purse to another
    /// </summary>
    [JsonConverter(typeof(TransferConverter))]
    public class Transfer
    {
        protected int _version;

        [JsonIgnore]
        public int Version
        {
            get { return _version; }
        }

        protected TransferV1 _transferV1;

        public static explicit operator TransferV1(Transfer transfer)
        {
            if(transfer._version == 1)
                return transfer._transferV1;

            throw new InvalidCastException("Version2 transfer cannot be converted to Version1");
        }
        
        public static explicit operator Transfer(TransferV1 transfer)
        {
            return new Transfer()
            {
                _version = 1,
                _transferV1 = transfer,
                Amount = transfer.Amount,
                From = new InitiatorAddr { AccountHash = transfer.From },
                To = transfer.To,
                Id = transfer.Id,
                TransactionHash = new TransactionHash() { Deploy = transfer.DeployHash },
                Source = transfer.Source,
                Target = transfer.Target,
                Gas = transfer.Gas,
            };
        }
        
        /// <summary>
        /// Transfer amount
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Amount { get; init; }
        
        /// <summary>
        /// Transaction that created the transfer
        /// </summary>
        [JsonPropertyName("transaction_hash")]
        public TransactionHash TransactionHash { get; init; }
        
        /// <summary>
        /// Entity from which transfer was executed
        /// </summary>
        [JsonPropertyName("from")]
        public InitiatorAddr From { get; init; }
        
        /// <summary>
        /// Gas
        /// </summary>
        [JsonPropertyName("gas")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Gas { get; init; }
        
        /// <summary>
        /// User-defined id
        /// </summary>
        [JsonPropertyName("id")]
        public ulong? Id { get; init; }
        
        /// <summary>
        /// Source purse
        /// </summary>
        [JsonPropertyName("source")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef Source { get; init; }
        
        /// <summary>
        /// Target purse
        /// </summary>
        [JsonPropertyName("target")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef Target { get; init; }
        
        /// <summary>
        /// Account to which funds are transferred
        /// </summary>
        [JsonPropertyName("to")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public AccountHashKey To { get; init; }

        public Transfer()
        {
            // this value is overriden in FromTransferV1 
            _version = 2;
        }
        
        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class TransferConverter : JsonConverter<Transfer>
        {
            public override Transfer Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        JsonElement root = doc.RootElement;

                        if (root.TryGetProperty("Version1", out JsonElement v1Element))
                        {
                            var transfer = JsonSerializer.Deserialize<TransferV1>(v1Element.GetRawText(), options);
                            return transfer != null ? (Transfer)transfer : null;
                        }

                        if (root.TryGetProperty("Version2", out JsonElement v2Element))
                        {
                            var transfer = JsonSerializer.Deserialize<TransferV2>(v2Element.GetRawText());
                            return transfer;
                        }

                        // try as Casper node v1.x for backward compatibility
                        var transferv1 = JsonSerializer.Deserialize<TransferV1>(root.GetRawText(), options);
                        return transferv1 != null ? (Transfer)transferv1 : null;
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                Transfer transfer,
                JsonSerializerOptions options)
            {
                switch (transfer.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize((TransferV1)transfer, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(transfer, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected transfer version {transfer.Version}");
                }
            }
        }
    }

    internal class TransferV2 : Transfer
    {
        
    }
}
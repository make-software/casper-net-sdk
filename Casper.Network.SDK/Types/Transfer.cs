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
    public class TransferV1 : Transfer
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

        public TransferV1()
        {
            _version = 1;
        }
    }
    
    /// <summary>
    /// Represents a version 2 transfer from one purse to another
    /// </summary>
    public class TransferV2 : Transfer
    {
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
        
        
        public TransferV2()
        {
            _version = 2;
        }
    }

    public interface ITransfer
    {
        public int Version { get; }
        public TransferV1 TransferV1 { get; }
        public TransferV2 TransferV2 { get; }
    }
    
    public class Transfer: ITransfer
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
        TransferV1 ITransfer.TransferV1 => this as TransferV1;

        /// <summary>
        /// Returns the transfer as a Version2 transfer object.
        /// </summary>
        TransferV2 ITransfer.TransferV2 => this as TransferV2;
        
        /// <summary>
        /// Json converter class to serialize/deserialize a Block to/from Json
        /// </summary>
        public class TransferConverter : JsonConverter<ITransfer>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(ITransfer) ||
                       typeToConvert == typeof(Transfer);
            }

            public override ITransfer Read(
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
                            return JsonSerializer.Deserialize<TransferV1>(v1Element.GetRawText(), options);

                        if (root.TryGetProperty("Version2", out JsonElement v2Element))
                            return JsonSerializer.Deserialize<TransferV2>(v2Element.GetRawText(), options);

                        // try as Casper node v1.x for backward compatibility
                        return JsonSerializer.Deserialize<TransferV1>(root.GetRawText(), options);
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                ITransfer transfer,
                JsonSerializerOptions options)
            {
                switch (transfer.Version)
                {
                    case 1:
                        writer.WritePropertyName("Version1");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(transfer as TransferV1, options);
                        writer.WriteEndObject();
                        break;
                    case 2:
                        writer.WritePropertyName("Version2");
                        writer.WriteStartObject();
                        JsonSerializer.Serialize(transfer as TransferV2, options);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException($"Unexpected transfer version {transfer.Version}");
                }
            }
        }
    }
}
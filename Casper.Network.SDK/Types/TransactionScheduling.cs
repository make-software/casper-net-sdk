using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public enum TransactionSchedulingType
    {
        Standard = 0,
        FutureEra = 1,
        FutureTimestamp = 2,
    }
    
    public class TransactionScheduling
    {
        public TransactionSchedulingType Type { get; init; }
        
        public ulong EraId { get; init; }
        
        public ulong Timestamp { get; init; }
        
        public class TransactionSchedulingConverter : JsonConverter<TransactionScheduling>
        {
            public override TransactionScheduling Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                TransactionScheduling scheduling;
                
                if (reader.TokenType == JsonTokenType.String)
                {
                    var schedulingType = reader.GetString();
                    switch (schedulingType)
                    {
                        case "Standard":
                            scheduling = new TransactionScheduling
                            {
                                Type = TransactionSchedulingType.Standard,
                            };
                            break;
                        default:
                            throw new JsonException("Cannot deserialize TransactionScheduling. Unknown scheduling type");
                    }
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read(); // skip start object
                    var schedulingType = reader.GetString();
                    reader.Read();
                    switch (schedulingType)
                    {
                        case "FutureEra":
                            scheduling = new TransactionScheduling
                            {
                                Type = TransactionSchedulingType.FutureEra,
                                EraId = reader.GetUInt64(),
                            };
                            break;
                        case "FutureTimestamp":
                            scheduling = new TransactionScheduling
                            {
                                Type = TransactionSchedulingType.FutureTimestamp,
                                Timestamp = reader.GetUInt64(),
                            };
                            break;
                        default:
                            throw new JsonException("Cannot deserialize TransactionScheduling. Unknown scheduling type");
                    }
                    reader.Read();
                }
                else 
                    throw new JsonException("Cannot deserialize TransactionScheduling.");

                return scheduling;
            }

            public override void Write(
                Utf8JsonWriter writer,
                TransactionScheduling value,
                JsonSerializerOptions options)
            {
                switch (value.Type)
                {
                    case TransactionSchedulingType.Standard:
                        writer.WriteStringValue("Standard");
                        break;
                    case TransactionSchedulingType.FutureEra:
                        writer.WriteStartObject();
                        writer.WriteNumber("FutureEra", value.EraId);
                        writer.WriteEndObject();
                        break;
                    case TransactionSchedulingType.FutureTimestamp:
                        writer.WriteStartObject();
                        writer.WriteString("FutureTimestamp", DateUtils.ToISOString(value.Timestamp));
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException("Cannot serialize due to unkown transaction scheduling type.");
                }
            }
        }
    }
}
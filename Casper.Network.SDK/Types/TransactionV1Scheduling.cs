using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public enum TransactionV1SchedulingType
    {
        Standard = 0,
        FutureEra = 1,
        FutureTimestamp = 2,
    }

    public interface ITransactionV1Scheduling
    {
    }
    
    public class StandardTransactionScheduling : ITransactionV1Scheduling
    {
    }

    public class StandardTransactionV1Scheduling : StandardTransactionScheduling
    {
    }
    
    public class FutureEraTransactionScheduling : ITransactionV1Scheduling
    {
        public ulong EraId { get; init; }
    }

    public class FutureEraTransactionV1Scheduling : FutureEraTransactionScheduling
    {
    }

    public class FutureTimestampTransactionScheduling : ITransactionV1Scheduling
    {
        public ulong Timestamp { get; init; }
    }
    
    public class FutureTimestampTransactionV1Scheduling : FutureTimestampTransactionScheduling
    {
    }

    public abstract class TransactionScheduling
    {
        public static ITransactionV1Scheduling Standard =>
            new StandardTransactionV1Scheduling();

        public static ITransactionV1Scheduling FutureEra(ulong eraId)
        {
            return new FutureEraTransactionV1Scheduling()
            {
                EraId = eraId,
            };
        }

        public static ITransactionV1Scheduling FutureTimestamp(ulong timestamp)
        {
            return new FutureTimestampTransactionV1Scheduling()
            {
                Timestamp = timestamp,
            };
        }
    }
    
    public class TransactionV1Scheduling : TransactionScheduling
    {
        public class TransactionV1SchedulingConverter : JsonConverter<ITransactionV1Scheduling>
        {
            public override ITransactionV1Scheduling Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                ITransactionV1Scheduling v1Scheduling;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var schedulingType = reader.GetString();
                    switch (schedulingType)
                    {
                        case "Standard":
                            v1Scheduling = new StandardTransactionV1Scheduling();
                            break;
                        default:
                            throw new JsonException(
                                "Cannot deserialize TransactionScheduling. Unknown scheduling type");
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
                            v1Scheduling = new FutureEraTransactionV1Scheduling
                            {
                                EraId = reader.GetUInt64(),
                            };
                            break;
                        case "FutureTimestamp":
                            v1Scheduling = new FutureTimestampTransactionV1Scheduling
                            {
                                Timestamp = DateUtils.ToEpochTime(reader.GetString()),
                            };
                            break;
                        default:
                            throw new JsonException(
                                "Cannot deserialize TransactionScheduling. Unknown scheduling type");
                    }

                    reader.Read();
                }
                else
                    throw new JsonException("Cannot deserialize TransactionScheduling.");

                return v1Scheduling;
            }

            public override void Write(
                Utf8JsonWriter writer,
                ITransactionV1Scheduling value,
                JsonSerializerOptions options)
            {
                switch (value)
                {
                    case StandardTransactionV1Scheduling:
                        writer.WriteStringValue("Standard");
                        break;
                    case FutureEraTransactionV1Scheduling eraScheduling:
                        writer.WriteStartObject();
                        writer.WriteNumber("FutureEra", eraScheduling.EraId);
                        writer.WriteEndObject();
                        break;
                    case FutureTimestampTransactionV1Scheduling timestampScheduling:
                        writer.WriteStartObject();
                        writer.WriteString("FutureTimestamp", DateUtils.ToISOString(timestampScheduling.Timestamp));
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException("Cannot serialize due to unkown transaction scheduling type.");
                }
            }
        }
    }
}
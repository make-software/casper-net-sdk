using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum InvocationTargetTag
    {
        ByHash = 0,
        ByName = 1,
        ByPackageHash = 2,
        ByPackageName = 3,
    }

    public class ByHashInvocationTarget : IInvocationTarget
    {
        public string Hash { get; init; }
    }

    public class ByNameInvocationTarget : IInvocationTarget
    {
        public string Name { get; init; }
    }

    public class ByPackageHashInvocationTarget : IInvocationTarget
    {
        [JsonPropertyName("addr")] public string Hash { get; init; }

        [JsonPropertyName("version")] public UInt32? Version { get; init; }
    }

    public class ByPackageNameInvocationTarget : IInvocationTarget
    {
        [JsonPropertyName("name")] public string Name { get; init; }

        [JsonPropertyName("version")] public UInt32? Version { get; init; }
    }

    [JsonConverter(typeof(InvocationTargetConverter))]
    public interface IInvocationTarget
    {
        public class InvocationTargetConverter : JsonConverter<IInvocationTarget>
        {
            public override IInvocationTarget Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                IInvocationTarget target;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read(); // skip start object
                    var idType = reader.GetString();
                    reader.Read();
                    switch (idType)
                    {
                        case "ByHash":
                            target = new ByHashInvocationTarget { Hash = reader.GetString() };
                            reader.Read();
                            break;
                        case "ByName":
                            target = new ByNameInvocationTarget { Name = reader.GetString() };
                            reader.Read();
                            break;
                        case "ByPackageHash":
                            target = JsonSerializer.Deserialize<ByPackageHashInvocationTarget>(ref reader, options);
                            break;
                        case "ByPackageName":
                            target = JsonSerializer.Deserialize<ByPackageNameInvocationTarget>(ref reader, options);
                            break;
                        default:
                            throw new JsonException(
                                "Cannot deserialize TransactionScheduling. Unknown scheduling type");
                    }

                    reader.Read(); // skip end object
                }
                else
                    throw new JsonException("Cannot deserialize TransactionScheduling.");

                return target;
            }

            public override void Write(
                Utf8JsonWriter writer,
                IInvocationTarget value,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                if (value is ByHashInvocationTarget byHash)
                    writer.WriteString("ByHash", byHash.Hash);
                else if (value is ByNameInvocationTarget byName)
                    writer.WriteString("ByName", byName.Name);
                else if (value is ByPackageHashInvocationTarget byPackageHash)
                {
                    writer.WritePropertyName("ByPackageHash");
                    JsonSerializer.Serialize(writer, byPackageHash);
                }
                else if (value is ByPackageNameInvocationTarget byPackageName)
                {
                    writer.WritePropertyName("ByPackageName");
                    JsonSerializer.Serialize(writer, byPackageName);
                }
                else
                    throw new JsonException("Unknown invocation target type.");

                writer.WriteEndObject();
            }
        }
    }

    public enum TransactionTargetType
    {
        Native = 0,
        Stored = 1,
        Session = 2,
    }

    public enum TransactionRuntime
    {
        /// <summary>
        /// The Casper Version 1 Virtual Machine.
        /// </summary>
        VmCasperV1,

        /// <summary>
        /// The Casper Version 2 Virtual Machine.
        /// </summary>
        VmCasperV2,
    }

    public interface ITransactionV1Target
    {
    }

    public class NativeTransactionV1Target : ITransactionV1Target
    {
    }

    public class StoredTransactionV1Target : ITransactionV1Target
    {
        [JsonPropertyName("id")] 
        public IInvocationTarget Id { get; init; }

        /// <summary>
        /// Targeted Casper VM version.
        /// </summary>
        [JsonPropertyName("runtime")]
        public TransactionRuntime Runtime { get; set; }
    }

    public class SessionTransactionV1Target : ITransactionV1Target
    {
        /// <summary>
        /// Wasm bytes for a Session transaction type.
        /// </summary>
        [JsonPropertyName("module_bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] ModuleBytes { get; init; }
        
        /// <summary>
        /// Targeted Casper VM version.
        /// </summary>
        [JsonPropertyName("runtime")]
        public TransactionRuntime Runtime { get; set; }
    }
    
    public class TransactionV1Target
    {
        public static ITransactionV1Target Native => new NativeTransactionV1Target();
        
        public static ITransactionV1Target StoredByHash(string contractHash)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByHashInvocationTarget { Hash = contractHash }
            };
        }

        public static ITransactionV1Target StoredByName(string name)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByNameInvocationTarget { Name = name }
            };
        }

        public static ITransactionV1Target StoredByPackageHash(string packageHash, UInt32? version = null)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByPackageHashInvocationTarget { Hash = packageHash, Version = version }
            };
        }

        public static ITransactionV1Target StoredByPackageName(string name, UInt32? version = null)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByPackageNameInvocationTarget() { Name = name, Version = version }
            };
        }

        public static ITransactionV1Target Session(byte[] moduleBytes)
        {
            return new SessionTransactionV1Target()
            {
                ModuleBytes = moduleBytes,
            };
        }

        public class TransactionTargetConverter : JsonConverter<ITransactionV1Target>
        {
            public override ITransactionV1Target Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var targetType = reader.GetString();

                    switch (targetType)
                    {
                        case "Native":
                            return new NativeTransactionV1Target();
                        default:
                            throw new JsonException($"TransactionTargetType '{targetType}' not supported.");
                    }
                }
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    ITransactionV1Target transactionTarget = null;
                    IInvocationTarget id = null;
                    string module_bytes = null;
                    TransactionRuntime runtime = TransactionRuntime.VmCasperV1;

                    reader.Read(); // skip start object
                    var targetType = reader.GetString();
                    reader.Read();

                    switch (targetType)
                    {
                        case "Stored":
                            reader.Read();
                            while (reader.TokenType != JsonTokenType.EndObject)
                            {
                                var prop = reader.GetString();
                                reader.Read();
                                switch (prop)
                                {
                                    case "id":
                                        id = JsonSerializer.Deserialize<IInvocationTarget>(ref reader, options);
                                        reader.Read();
                                        break;
                                    case "runtime":
                                        runtime = EnumCompat.Parse<TransactionRuntime>(reader.GetString());
                                        reader.Read(); // skip end object
                                        break;
                                }
                            }

                            reader.Read(); // skip end object

                            transactionTarget = new StoredTransactionV1Target()
                            {
                                Id = id,
                                Runtime = runtime,
                            };
                            break;
                        case "Session":
                            reader.Read();
                            while (reader.TokenType != JsonTokenType.EndObject)
                            {
                                var prop = reader.GetString();
                                reader.Read();
                                switch (prop)
                                {
                                    case "module_bytes":
                                        module_bytes = reader.GetString();
                                        break;
                                    case "runtime":
                                        runtime = EnumCompat.Parse<TransactionRuntime>(reader.GetString());
                                        break;
                                }
                            }

                            reader.Read(); // skip end object

                            transactionTarget = new SessionTransactionV1Target()
                            {
                                ModuleBytes = Hex.Decode(module_bytes),
                                Runtime = runtime,
                            };
                            break;
                        default:
                            throw new JsonException($"TransactionTargetType '{targetType}' not supported.");
                    }

                    return transactionTarget;
                }

                throw new JsonException("Cannot deserialize TransactionTarget. PropertyName expected");
            }

            public override void Write(
                Utf8JsonWriter writer,
                ITransactionV1Target value,
                JsonSerializerOptions options)
            {
                switch (value)
                {
                    case NativeTransactionV1Target:
                        writer.WriteStringValue("Native");
                        break;
                    case StoredTransactionV1Target storedTarget:
                        writer.WriteStartObject();
                        writer.WriteStartObject("Stored");
                        writer.WritePropertyName("id");
                        JsonSerializer.Serialize(writer, storedTarget.Id);
                        writer.WriteString("runtime", storedTarget.Runtime.ToString());
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        break;
                    case SessionTransactionV1Target sessionTarget:
                        writer.WriteStartObject();
                        writer.WriteStartObject("Session");
                        writer.WriteString("module_bytes", Hex.ToHexString(sessionTarget.ModuleBytes));
                        writer.WriteString("runtime", sessionTarget.Runtime.ToString());
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException("Cannot serialize empty transaction target.");
                }
            }
        }
    }
}
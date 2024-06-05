using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
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
        [JsonPropertyName("addr")] public string Addr { get; init; }

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

    public enum TransactionSessionKind
    {
        /// <summary>
        /// A standard (non-special-case) session.
        /// This kind of session is not allowed to install or upgrade a stored contract,
        /// but can call stored contracts.
        /// </summary>
        Standard,

        /// <summary>
        /// A session which installs a stored contract.
        /// </summary>
        Installer,

        /// <summary>
        /// A session which upgrades a previously-installed stored contract.
        /// Such a session must have \"package_id: PackageIdentifier\" runtime arg present.
        /// </summary>
        Upgrader,

        /// <summary>
        /// A session which doesn't call any stored contracts.
        /// This kind of session is not allowed to install or upgrade a stored contract.
        /// </summary>
        Isolated,
    }

    public class TransactionTarget
    {
        public TransactionTargetType Type { get; init; }

        [JsonPropertyName("id")] public IInvocationTarget Id { get; init; }

        public TransactionSessionKind SessionKind { get; init; }

        /// <summary>
        /// wasm Bytes
        /// </summary>
        [JsonPropertyName("module_bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] ModuleBytes { get; init; }

        [JsonPropertyName("runtime")] public TransactionRuntime Runtime { get; set; }

        public static TransactionTarget StoredByHash(string hash)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Stored,
                Id = new ByHashInvocationTarget { Hash = hash }
            };
        }

        public static TransactionTarget StoredByName(string name)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Stored,
                Id = new ByNameInvocationTarget { Name = name }
            };
        }

        public static TransactionTarget StoredByPackageHash(string hash, UInt32? version = null)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Stored,
                Id = new ByPackageHashInvocationTarget { Addr = hash, Version = version }
            };
        }

        public static TransactionTarget StoredByPackageName(string name, UInt32? version = null)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Stored,
                Id = new ByPackageNameInvocationTarget() { Name = name, Version = version }
            };
        }

        public static TransactionTarget StandardSession(byte[] moduleBytes)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Session,
                SessionKind = TransactionSessionKind.Standard,
                ModuleBytes = moduleBytes,
            };
        }

        public static TransactionTarget InstallerSession(byte[] moduleBytes)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Session,
                SessionKind = TransactionSessionKind.Installer,
                ModuleBytes = moduleBytes,
            };
        }

        public static TransactionTarget UpgraderSession(byte[] moduleBytes)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Session,
                SessionKind = TransactionSessionKind.Upgrader,
                ModuleBytes = moduleBytes,
            };
        }

        public static TransactionTarget IsolatedSession(byte[] moduleBytes)
        {
            return new TransactionTarget()
            {
                Type = TransactionTargetType.Session,
                SessionKind = TransactionSessionKind.Isolated,
                ModuleBytes = moduleBytes,
            };
        }

        public class TransactionTargetConverter : JsonConverter<TransactionTarget>
        {
            public override TransactionTarget Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var targetType = reader.GetString();

                    var type = EnumCompat.Parse<TransactionTargetType>(targetType);
                    switch (targetType)
                    {
                        case "Native":
                            return new TransactionTarget
                            {
                                Type = TransactionTargetType.Native,
                            };
                        default:
                            throw new JsonException($"TransactionTargetType '{targetType}' not supported.");
                    }
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    TransactionTarget transactionTarget = null;
                    IInvocationTarget id = null;
                    string kind = null;
                    string module_bytes = null;
                    string runtime = null;
      
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
                                        runtime = reader.GetString();
                                        reader.Read(); // skip end object
                                        break;
                                }
                            }

                            reader.Read(); // skip end object

                            transactionTarget = new TransactionTarget()
                            {
                                Type = EnumCompat.Parse<TransactionTargetType>(targetType), 
                                Id = id,
                            };
                            if (runtime != null)
                                transactionTarget.Runtime = EnumCompat.Parse<TransactionRuntime>(runtime);
                            break;
                        case "Session":
                            reader.Read();
                            while (reader.TokenType != JsonTokenType.EndObject)
                            {
                                var prop = reader.GetString();
                                reader.Read();
                                switch (prop)
                                {
                                    case "kind":
                                        kind = reader.GetString();
                                        break;
                                    case "module_bytes":
                                        module_bytes = reader.GetString();
                                        break;
                                    case "runtime":
                                        runtime = reader.GetString();
                                        break;
                                }
                            }

                            reader.Read(); // skip end object
                            
                            transactionTarget = new TransactionTarget()
                            {
                                Type = EnumCompat.Parse<TransactionTargetType>(targetType), 
                                SessionKind = EnumCompat.Parse<TransactionSessionKind>(kind),
                                ModuleBytes = Hex.Decode(module_bytes),
                                Runtime = EnumCompat.Parse<TransactionRuntime>(runtime),
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
                TransactionTarget value,
                JsonSerializerOptions options)
            {
                switch (value.Type)
                {
                    case TransactionTargetType.Native:
                        writer.WriteStringValue("Native");
                        break;
                    case TransactionTargetType.Stored:
                        writer.WriteStartObject();
                        writer.WriteStartObject("Stored");
                        writer.WritePropertyName("id");
                        JsonSerializer.Serialize(writer, value.Id);
                        writer.WriteString("runtime", value.Runtime.ToString());
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        break;
                    case TransactionTargetType.Session:
                        writer.WriteStartObject();
                        writer.WriteStartObject("Session");
                        writer.WriteString("kind", value.SessionKind.ToString());
                        writer.WriteString("module_bytes", Hex.ToHexString(value.ModuleBytes));
                        writer.WriteString("runtime", value.Runtime.ToString());
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
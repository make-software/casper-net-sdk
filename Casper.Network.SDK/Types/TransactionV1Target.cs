using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
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

        const ushort TAG_FIELD_INDEX = 0;
        const byte BY_HASH_VARIANT = 0;
        const ushort BY_HASH_HASH_INDEX = 1;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { BY_HASH_VARIANT })
                .AddField(BY_HASH_HASH_INDEX, Hex.Decode(Hash))
                .GetBytes();
        }
    }

    public class ByNameInvocationTarget : IInvocationTarget
    {
        public string Name { get; init; }

        const ushort TAG_FIELD_INDEX = 0;
        const byte BY_NAME_VARIANT = 1;
        const ushort BY_NAME_NAME_INDEX = 1;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { BY_NAME_VARIANT })
                .AddField(BY_NAME_NAME_INDEX, CLValue.String(Name))
                .GetBytes();
        }
    }

    public class ByPackageHashInvocationTarget : IInvocationTarget
    {
        [JsonPropertyName("addr")] public string Hash { get; init; }

        [JsonPropertyName("version")] public UInt32? Version { get; init; }
        
        [JsonPropertyName("protocol_version_major")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UInt32? ProtocolVersionMajor { get; init; }

        const ushort TAG_FIELD_INDEX = 0;
        const byte BY_PACKAGE_HASH_VARIANT = 2;
        const ushort BY_PACKAGE_HASH_ADDR_INDEX = 1;
        const ushort BY_PACKAGE_HASH_VERSION_INDEX = 2;
        const ushort BY_PACKAGE_HASH_PROTOCOL_VERSION_MAJOR_INDEX = 3;

        public byte[] ToBytes()
        {
            var calltable = new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { BY_PACKAGE_HASH_VARIANT })
                .AddField(BY_PACKAGE_HASH_ADDR_INDEX, Hex.Decode(Hash))
                .AddField(BY_PACKAGE_HASH_VERSION_INDEX, Version.HasValue
                    ? CLValue.Option(CLValue.U32(Version.Value))
                    : CLValue.OptionNone(CLType.U32));
            
            if (ProtocolVersionMajor.HasValue)
                calltable.AddField(BY_PACKAGE_HASH_PROTOCOL_VERSION_MAJOR_INDEX,
                    CLValue.U32(ProtocolVersionMajor.Value));
            
            return calltable.GetBytes();
        }
    }

    public class ByPackageNameInvocationTarget : IInvocationTarget
    {
        [JsonPropertyName("name")] public string Name { get; init; }

        [JsonPropertyName("version")] public UInt32? Version { get; init; }
        
        [JsonPropertyName("protocol_version_major")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UInt32? ProtocolVersionMajor { get; init; }

        const ushort TAG_FIELD_INDEX = 0;
        const byte BY_PACKAGE_NAME_VARIANT = 3;
        const ushort BY_PACKAGE_NAME_NAME_INDEX = 1;
        const ushort BY_PACKAGE_NAME_VERSION_INDEX = 2;
        const ushort BY_PACKAGE_HASH_PROTOCOL_VERSION_MAJOR_INDEX = 3;

        public byte[] ToBytes()
        {
            var calltable =  new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { BY_PACKAGE_NAME_VARIANT })
                .AddField(BY_PACKAGE_NAME_NAME_INDEX, CLValue.String(Name))
                .AddField(BY_PACKAGE_NAME_VERSION_INDEX, Version.HasValue
                    ? CLValue.Option(CLValue.U32(Version.Value))
                    : CLValue.OptionNone(CLType.U32));
            
            if (ProtocolVersionMajor.HasValue)
                calltable.AddField(BY_PACKAGE_HASH_PROTOCOL_VERSION_MAJOR_INDEX,
                    CLValue.U32(ProtocolVersionMajor.Value));
            
            return calltable.GetBytes();
        }
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

        public byte[] ToBytes();
    }

    public enum TransactionTargetType
    {
        Native = 0,
        Stored = 1,
        Session = 2,
    }

    public class TransactionRuntime
    {
        private const byte VM_CASPER_V1_TAG = 0;
        private const byte VM_CASPER_V2_TAG = 1;
        private byte _tag = VM_CASPER_V1_TAG;

        public static TransactionRuntime FromString(string json)
        {
            switch (json)
            {
                case "VmCasperV1":
                    return VmCasperV1();
                case "VmCasperV2":
                    return VmCasperV2();
                default:
                    throw new JsonException($"Unknown TransactionRuntime '{json}'");
            }
        }

        public override string ToString()
        {
            switch (_tag)
            {
                case VM_CASPER_V1_TAG:
                    return "VmCasperV1";
                case VM_CASPER_V2_TAG:
                    return "VmCasperV2";
                default:
                    throw new JsonException($"Unknown TransactionRuntime '{_tag}'");
            }
        }

        /// <summary>
        /// The Casper Version 1 Virtual Machine.
        /// </summary>
        public static TransactionRuntime VmCasperV1()
        {
            return new TransactionRuntime() { _tag = VM_CASPER_V1_TAG };
        }

        /// <summary>
        /// The Casper Version 2 Virtual Machine.
        /// </summary>
        public static TransactionRuntime VmCasperV2()
        {
            return new TransactionRuntime() { _tag = VM_CASPER_V2_TAG };
        }

        const ushort TAG_FIELD_INDEX = 0;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { _tag })
                .GetBytes();
        }
    }

    public interface ITransactionV1Target
    {
        public byte[] ToBytes();
    }

    public class NativeTransactionV1Target : ITransactionV1Target
    {
        const ushort TAG_FIELD_INDEX = 0;
        const byte NATIVE_VARIANT = 0;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { NATIVE_VARIANT })
                .GetBytes();
        }
    }

    public class StoredTransactionV1Target : ITransactionV1Target
    {
        [JsonPropertyName("id")] 
        public IInvocationTarget Id { get; set; }

        /// <summary>
        /// Targeted Casper VM version.
        /// </summary>
        [JsonPropertyName("runtime")]
        public TransactionRuntime Runtime { get; set; }

        public StoredTransactionV1Target()
        {
            Runtime = TransactionRuntime.VmCasperV1();
        }

        const ushort TAG_FIELD_INDEX = 0;
        const byte STORED_VARIANT = 1;
        const ushort STORED_ID_INDEX = 1;
        const ushort STORED_RUNTIME_INDEX = 2;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { STORED_VARIANT })
                .AddField(STORED_ID_INDEX, Id.ToBytes())
                .AddField(STORED_RUNTIME_INDEX, Runtime.ToBytes())
                .GetBytes();
        }
    }

    public class SessionTransactionV1Target : ITransactionV1Target
    {
        /// <summary>
        /// Flag determining if the Wasm is an install/upgrade.
        /// </summary>
        [JsonPropertyName("is_install_upgrade")]
        public bool IsInstallUpgrade { get; set; }

        /// <summary>
        /// Wasm bytes for a Session transaction type.
        /// </summary>
        [JsonPropertyName("module_bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] ModuleBytes { get; set; }

        /// <summary>
        /// Targeted Casper VM version.
        /// </summary>
        [JsonPropertyName("runtime")]
        public TransactionRuntime Runtime { get; set; }

        public SessionTransactionV1Target()
        {
            Runtime = TransactionRuntime.VmCasperV1();
        }

        const ushort TAG_FIELD_INDEX = 0;
        const byte SESSION_VARIANT = 2;
        const ushort SESSION_IS_INSTALL_INDEX = 1;
        const ushort SESSION_RUNTIME_INDEX = 2;
        const ushort SESSION_MODULE_BYTES_INDEX = 3;

        public byte[] ToBytes()
        {
            var ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(ModuleBytes.Length));
            ms.Write(ModuleBytes);

            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] { SESSION_VARIANT })
                .AddField(SESSION_IS_INSTALL_INDEX, new byte[] { IsInstallUpgrade ? (byte)0x01 : (byte)0x00 })
                .AddField(SESSION_RUNTIME_INDEX, Runtime.ToBytes())
                .AddField(SESSION_MODULE_BYTES_INDEX, ms.ToArray())
                .GetBytes();
        }
    }

    public class TransactionV1Target
    {
        public static ITransactionV1Target Native => new NativeTransactionV1Target();

        public static StoredTransactionV1Target StoredByHash(string contractHash)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByHashInvocationTarget { Hash = contractHash },
            };
        }

        public static StoredTransactionV1Target StoredByName(string name)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByNameInvocationTarget { Name = name },
            };
        }

        public static StoredTransactionV1Target StoredByPackageHash(string packageHash, UInt32? version = null, UInt32? protocolVersionMajor = null)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByPackageHashInvocationTarget { Hash = packageHash, Version = version, ProtocolVersionMajor = protocolVersionMajor },
            };
        }

        public static StoredTransactionV1Target StoredByPackageName(string name, UInt32? version = null, UInt32? protocolVersionMajor = null)
        {
            return new StoredTransactionV1Target()
            {
                Id = new ByPackageNameInvocationTarget() { Name = name, Version = version, ProtocolVersionMajor = protocolVersionMajor },
            };
        }

        public static SessionTransactionV1Target Session(byte[] moduleBytes)
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
                    bool is_install_upgrade = false;
                    TransactionRuntime runtime = TransactionRuntime.VmCasperV1();

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
                                        runtime = TransactionRuntime.FromString(reader.GetString());
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
                                    case "is_install_upgrade":
                                        is_install_upgrade = reader.GetBoolean();
                                        reader.Read();
                                        break;
                                    case "module_bytes":
                                        module_bytes = reader.GetString();
                                        reader.Read();
                                        break;
                                    case "runtime":
                                        runtime = TransactionRuntime.FromString(reader.GetString());
                                        reader.Read();
                                        break;
                                }
                            }

                            reader.Read(); // skip end object

                            transactionTarget = new SessionTransactionV1Target()
                            {
                                IsInstallUpgrade = is_install_upgrade,
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
                        writer.WriteBoolean("is_install_upgrade", sessionTarget.IsInstallUpgrade);
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
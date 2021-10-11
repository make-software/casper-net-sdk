using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Numerics;
using NetCasperSDK.Converters;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperSDK.Types
{
    public abstract class ExecutableDeployItem
    {
        [JsonPropertyName("args")]
        public List<NamedArg> RuntimeArgs { get; protected set; }

        public abstract byte Tag();

        public abstract string JsonPropertyName();
    }

    public class ModuleBytesDeployItem : ExecutableDeployItem
    {
        [JsonPropertyName("module_bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] ModuleBytes { get; }

        public ModuleBytesDeployItem(byte[] moduleBytes)
        {
            RuntimeArgs = new List<NamedArg>();
            ModuleBytes = moduleBytes;
        }

        public ModuleBytesDeployItem(BigInteger amount)
        {
            RuntimeArgs = new List<NamedArg>();
            RuntimeArgs.Add(new NamedArg("amount", CLValue.U512(amount)));
            ModuleBytes = Array.Empty<byte>();
        }

        public override byte Tag() => 0;

        public override string JsonPropertyName() => "ModuleBytes";
    }

    public class StoredContractByHashDeployItem : ExecutableDeployItem
    {
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] Hash { get; }

        [JsonPropertyName("entry_point")] public string EntryPoint { get; } = "";

        public StoredContractByHashDeployItem(string hash, string entryPoint, List<NamedArg> args = null)
        {
            Hash = Hex.Decode(hash);
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 1;

        public override string JsonPropertyName() => "StoredContractByHash";
    }

    public class StoredContractByNameDeployItem : ExecutableDeployItem
    {
        [JsonPropertyName("name")] public string Name { get; } = "";

        [JsonPropertyName("entry_point")] public string EntryPoint { get; } = "";

        public StoredContractByNameDeployItem(string name, string entryPoint, List<NamedArg> args = null)
        {
            Name = name;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 2;

        public override string JsonPropertyName() => "StoredContractByName";
    }

    public class StoredVersionedContractByHashDeployItem : ExecutableDeployItem
    {
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] Hash { get; }

        [JsonPropertyName("version")] public uint? Version { get; }

        [JsonPropertyName("entry_point")] public string EntryPoint { get; } = "";

        public StoredVersionedContractByHashDeployItem(string hash, uint? version, string entryPoint,
            List<NamedArg> args = null)
        {
            Hash = Hex.Decode(hash);
            Version = version;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 3;

        public override string JsonPropertyName() => "StoredVersionedContractByHash";
    }

    public class StoredVersionedContractByNameDeployItem : ExecutableDeployItem
    {
        [JsonPropertyName("name")] public string Name { get; } = "";

        [JsonPropertyName("version")] public uint? Version { get; }

        [JsonPropertyName("entry_point")] public string EntryPoint { get; } = "";

        public StoredVersionedContractByNameDeployItem(string name, uint? version, string entryPoint,
            List<NamedArg> args = null)
        {
            Name = name;
            Version = version;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 4;

        public override string JsonPropertyName() => "StoredVersionedContractByName";
    }

    public class TransferDeployItem : ExecutableDeployItem
    {
        public TransferDeployItem(BigInteger amount, PublicKey target, CLValue sourcePurse, ulong? id = null)
        {
            var targetHash = target.GetAccountHash();

            RuntimeArgs = new List<NamedArg>();
            RuntimeArgs.Add(new NamedArg("amount", CLValue.U512(amount)));
            RuntimeArgs.Add(new NamedArg("target", CLValue.ByteArray(targetHash)));
            if (id != null)
                RuntimeArgs.Add(new NamedArg("id", CLValue.Option(CLValue.U64(id ?? 0))));
        }

        public override byte Tag() => 5;

        public override string JsonPropertyName() => "Transfer";
    }
}
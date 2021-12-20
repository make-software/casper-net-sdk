using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Numerics;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Base class for the possible variants of an executable deploy.
    /// </summary>
    public abstract class ExecutableDeployItem
    {
        /// <summary>
        /// List of runtime arguments.
        /// </summary>
        [JsonPropertyName("args")]
        [JsonConverter(typeof(GenericListConverter<NamedArg, NamedArg.NamedArgConverter>))]
        public List<NamedArg> RuntimeArgs { get; init; }

        public abstract byte Tag();

        public abstract string JsonPropertyName();
    }

    /// <summary>
    /// Deploy item with the capacity to contain executable code (e.g. a contract).
    /// </summary>
    public class ModuleBytesDeployItem : ExecutableDeployItem
    {
        /// <summary>
        /// wasm Bytes
        /// </summary>
        [JsonPropertyName("module_bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] ModuleBytes { get; init; }

        public ModuleBytesDeployItem()
        {
        }

        /// <summary>
        /// Creates a deploy item with binary code to execute and a list of named arguments.
        /// </summary>
        /// <param name="moduleBytes"></param>
        /// <param name="args"></param>
        public ModuleBytesDeployItem(byte[] moduleBytes, List<NamedArg> args = null)
        {
            ModuleBytes = moduleBytes;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        /// <summary>
        /// Creates a deploy item to specify a payment amount with origin the main purse
        /// of the caller's account.
        /// </summary>
        /// <param name="amount">Payment amount in motes</param>
        public ModuleBytesDeployItem(BigInteger amount)
        {
            RuntimeArgs = new List<NamedArg>();
            RuntimeArgs.Add(new NamedArg("amount", CLValue.U512(amount)));
            ModuleBytes = Array.Empty<byte>();
        }

        public override byte Tag() => 0;

        public override string JsonPropertyName() => "ModuleBytes";
    }

    /// <summary>
    /// Deploy item to call an entry point in a contract. The contract is referenced
    /// by its hash.
    /// </summary>
    public class StoredContractByHashDeployItem : ExecutableDeployItem
    {
        /// <summary>
        /// Hash of the contract.
        /// </summary>
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(CEP57Checksum.HashWithChecksumConverter))]
        public string Hash { get; init; }

        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonPropertyName("entry_point")] 
        public string EntryPoint { get; init; } = "";

        public StoredContractByHashDeployItem()
        {
        }
        
        /// <summary>
        /// Creates a deploy item to call an entry point in a contract. The contract is referenced
        /// by the contract hash.
        /// </summary>
        /// <param name="hash">Hash of the contract.</param>
        /// <param name="entryPoint">Method in the contract to call.</param>
        /// <param name="args">List of named arguments to pass as input data to the call.</param>
        public StoredContractByHashDeployItem(string hash, string entryPoint, List<NamedArg> args = null)
        {
            Hash = hash;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 1;

        public override string JsonPropertyName() => "StoredContractByHash";
    }

    /// <summary>
    /// Deploy item to call an entry point in a contract. The contract is referenced
    /// by a named key in the caller account pointing to the contract hash.
    /// </summary>
    public class StoredContractByNameDeployItem : ExecutableDeployItem
    {
        /// <summary>
        /// Name of a named key in the caller account that stores the contract hash.
        /// </summary>
        [JsonPropertyName("name")] 
        public string Name { get; init; } = "";

        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonPropertyName("entry_point")] 
        public string EntryPoint { get; init; } = "";

        public StoredContractByNameDeployItem()
        {
        }
        
        /// <summary>
        /// Creates a deploy item to call an entry point in a contract. The contract is referenced
        /// by a named key in the caller's account.
        /// </summary>
        /// <param name="name">Name of a named key in the caller account Named Keys that points to the contract.</param>
        /// <param name="entryPoint">Method in the contract to call.</param>
        /// <param name="args">List of named arguments to pass as input data to the call.</param>
        public StoredContractByNameDeployItem(string name, string entryPoint, List<NamedArg> args = null)
        {
            Name = name;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 2;

        public override string JsonPropertyName() => "StoredContractByName";
    }
    
    /// <summary>
    /// Deploy item to call an entry point in a contract. The contract is referenced
    /// by a contract package hash and a version number.
    /// </summary> 
    public class StoredVersionedContractByHashDeployItem : ExecutableDeployItem
    {
        /// <summary>
        /// Hash of the contract package.
        /// </summary>
        [JsonPropertyName("hash")]
        [JsonConverter(typeof(CEP57Checksum.HashWithChecksumConverter))]
        public string Hash { get; init; }

        /// <summary>
        /// Version of the contract to call (null indicates latest version).
        /// </summary>
        [JsonPropertyName("version")] 
        public uint? Version { get; init; }

        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonPropertyName("entry_point")] 
        public string EntryPoint { get; init; } = "";
        
        public StoredVersionedContractByHashDeployItem()
        {
        }
        
        /// <summary>
        /// Creates a deploy item to call an entry point in a contract package. The contract is referenced
        /// by the contract package hash and a version number in the contract package.
        /// </summary>
        /// <param name="hash">Hash of the contract package.</param>
        /// <param name="version">Version of the contract to call. Null for latest version.</param>
        /// <param name="entryPoint">Method in the contract to call.</param>
        /// <param name="args">List of named arguments to pass as input data to the call.</param>
        public StoredVersionedContractByHashDeployItem(string hash, uint? version, string entryPoint,
            List<NamedArg> args = null)
        {
            Hash = hash;
            Version = version;
            EntryPoint = entryPoint;
            RuntimeArgs = args ?? new List<NamedArg>();
        }

        public override byte Tag() => 3;

        public override string JsonPropertyName() => "StoredVersionedContractByHash";
    }

    /// <summary>
    /// Deploy item to call an entry point in a contract. The contract is referenced
    /// by a named key in the caller account pointing to the contract package hash
    /// and a version number.
    /// </summary>
    public class StoredVersionedContractByNameDeployItem : ExecutableDeployItem
    {
        /// <summary>
        /// Name of a named key in the caller account that stores the contract package hash.
        /// </summary>
        [JsonPropertyName("name")] 
        public string Name { get; init; } = "";

        /// <summary>
        /// Version of the contract to call (null indicates latest version).
        /// </summary>
        [JsonPropertyName("version")] 
        public uint? Version { get; init; }

        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonPropertyName("entry_point")] 
        public string EntryPoint { get; init; } = "";

        public StoredVersionedContractByNameDeployItem()
        {
        }
        
        /// <summary>
        /// Creates a deploy item to call an entry point in a contract package. The contract is referenced
        /// by a named key in the caller's account and a version number in the contract package.
        /// </summary>
        /// <param name="name">Name of a named key in the caller account Named Keys that points to the contract package.</param>
        /// <param name="version">Version of the contract to call. Null for latest version.</param>
        /// <param name="entryPoint">Method in the contract to call.</param>
        /// <param name="args">List of named arguments to pass as input data to the call.</param>
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

    /// <summary>
    /// Deploy item for transferring funds to a target account. 
    /// </summary>
    public class TransferDeployItem : ExecutableDeployItem
    {
        public TransferDeployItem()
        {
        }
        
        public TransferDeployItem(BigInteger amount, AccountHashKey targetAccountHash, ulong? id = null)
        {
            RuntimeArgs = new List<NamedArg>();
            RuntimeArgs.Add(new NamedArg("amount", CLValue.U512(amount)));
            RuntimeArgs.Add(new NamedArg("target", CLValue.ByteArray(targetAccountHash.RawBytes)));

            var optionValue = id == null ? CLValue.OptionNone(new CLTypeInfo(CLType.U64)) : CLValue.Option(CLValue.U64((ulong) id));
            RuntimeArgs.Add(new NamedArg("id", optionValue));
        }

        public override byte Tag() => 5;

        public override string JsonPropertyName() => "Transfer";
    }
}
using System;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public enum ByteCodeKind {
        /// <summary>
        /// Empty byte code.
        /// </summary>
        Empty = 0,
        /// <summary>
        /// Byte code to be executed with the version 1 Casper execution engine.
        /// </summary>
        V1CasperWasm = 1,
    }
    
    public static class ByteCodeKindExtensions
    {
        public static string ToKeyPrefix(this ByteCodeKind kind)
        {
            switch (kind)
            {
                case ByteCodeKind.Empty:
                    return "byte-code-empty-";
                case ByteCodeKind.V1CasperWasm:
                    return "byte-code-v1-wasm-";
                default:
                    return kind.ToString();
            }
        }
    }
    
    public class ByteCodeKey : GlobalStateKey
    {
        public ByteCodeKind Kind { get; init; }
        
        private static string GetPrefix(string key)
        {
            if (key.StartsWith(ByteCodeKind.Empty.ToKeyPrefix()))
                return ByteCodeKind.Empty.ToKeyPrefix();
            if (key.StartsWith(ByteCodeKind.V1CasperWasm.ToKeyPrefix()))
                return ByteCodeKind.V1CasperWasm.ToKeyPrefix();
            throw new Exception("Unexpected key prefix in ByteCodeKey: " + key);
        }
        
        public ByteCodeKey(string key) : base(key, ByteCodeKey.GetPrefix(key))
        {
            KeyIdentifier = KeyIdentifier.ByteCode;
            var prefix = GetPrefix(key);
            if (ByteCodeKind.Empty.ToKeyPrefix().Equals(prefix))
                Kind = ByteCodeKind.Empty;
            else if (ByteCodeKind.V1CasperWasm.ToKeyPrefix().Equals(prefix))
                Kind = ByteCodeKind.V1CasperWasm;
        }
    
        public ByteCodeKey(byte[] key) : this( 
            key[0] == (byte)ByteCodeKind.Empty
                ? ByteCodeKind.Empty.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                : (key[0] == (byte)ByteCodeKind.V1CasperWasm
                    ? ByteCodeKind.V1CasperWasm.ToKeyPrefix() + CEP57Checksum.Encode(key.Slice(1))
                    :  throw new Exception($"Wrong kind tag '{key[0]}' for ByteCodeKey.")))
        {
        }
    }
}
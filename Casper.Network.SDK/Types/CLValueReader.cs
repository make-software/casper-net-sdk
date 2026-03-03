using System;
using System.IO;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Reads a <see cref="CLValue"/> from a binary stream given a <see cref="CLTypeInfo"/> descriptor.
    /// The caller is responsible for knowing the type layout of the stream; no type tag bytes are read.
    /// </summary>
    public class CLValueReader
    {
        private readonly BinaryReader _reader;

        public CLValueReader(BinaryReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        /// Reads from the underlying stream the bytes that represent a value of the given type
        /// and returns a <see cref="CLValue"/> constructed from those bytes and the provided type info.
        /// </summary>
        public CLValue Read(CLTypeInfo typeInfo)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            using var ms = new MemoryStream();
            AppendValueBytes(ms, typeInfo);
            return new CLValue(ms.ToArray(), typeInfo);
        }

        private void AppendValueBytes(MemoryStream ms, CLTypeInfo typeInfo)
        {
            switch (typeInfo.Type)
            {
                case CLType.Bool:
                case CLType.U8:
                {
                    ms.WriteByte(_reader.ReadByte());
                    break;
                }

                case CLType.I32:
                case CLType.U32:
                {
                    var bytes = _reader.ReadBytes(4);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.I64:
                case CLType.U64:
                {
                    var bytes = _reader.ReadBytes(8);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.U128:
                case CLType.U256:
                case CLType.U512:
                {
                    // length-prefixed little-endian big integer: 1 byte length + N bytes
                    var len = _reader.ReadByte();
                    ms.WriteByte(len);
                    if (len > 0)
                    {
                        var bytes = _reader.ReadBytes(len);
                        ms.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }

                case CLType.Unit:
                    break; // zero bytes on the wire

                case CLType.String:
                {
                    // 4-byte LE length prefix followed by UTF-8 bytes
                    var lenBytes = _reader.ReadBytes(4);
                    ms.Write(lenBytes, 0, lenBytes.Length);
                    var len = ToInt32LE(lenBytes);
                    if (len > 0)
                    {
                        var strBytes = _reader.ReadBytes(len);
                        ms.Write(strBytes, 0, strBytes.Length);
                    }
                    break;
                }

                case CLType.URef:
                {
                    // 32-byte key + 1-byte access rights
                    var bytes = _reader.ReadBytes(33);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.PublicKey:
                {
                    // 1-byte algo prefix (already included in key size) + key bytes
                    int algo = _reader.PeekChar();
                    var keyLen = algo == 0x01
                        ? KeyAlgo.ED25519.GetKeySizeInBytes()
                        : KeyAlgo.SECP256K1.GetKeySizeInBytes();
                    var bytes = _reader.ReadBytes(keyLen);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.Key:
                {
                    // 1-byte tag + variant payload; mirrors GlobalStateKey.ReadCLGlobalStateKey
                    int keyId = _reader.PeekChar();
                    int keyLen;
                    if (keyId == (char)KeyIdentifier.EraInfo)
                        keyLen = 9;  // 1 tag + 8 (u64)
                    else if (keyId == (char)KeyIdentifier.URef)
                        keyLen = 34; // 1 tag + 33 (URef)
                    else
                        keyLen = 33; // 1 tag + 32 (hash)
                    var bytes = _reader.ReadBytes(keyLen);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.Option:
                {
                    if (typeInfo is not CLOptionTypeInfo optionTypeInfo)
                        throw new Exception("Expected CLOptionTypeInfo for Option CLType.");
                    var tag = _reader.ReadByte();
                    ms.WriteByte(tag);
                    if (tag != 0x00) // Some variant – read the inner value
                        AppendValueBytes(ms, optionTypeInfo.OptionType);
                    break;
                }

                case CLType.List:
                {
                    if (typeInfo is not CLListTypeInfo listTypeInfo)
                        throw new Exception("Expected CLListTypeInfo for List CLType.");
                    var countBytes = _reader.ReadBytes(4);
                    ms.Write(countBytes, 0, countBytes.Length);
                    var count = ToInt32LE(countBytes);
                    for (var i = 0; i < count; i++)
                        AppendValueBytes(ms, listTypeInfo.ListType);
                    break;
                }

                case CLType.ByteArray:
                {
                    if (typeInfo is not CLByteArrayTypeInfo baTypeInfo)
                        throw new Exception("Expected CLByteArrayTypeInfo for ByteArray CLType.");
                    var bytes = _reader.ReadBytes(baTypeInfo.Size);
                    ms.Write(bytes, 0, bytes.Length);
                    break;
                }

                case CLType.Result:
                {
                    if (typeInfo is not CLResultTypeInfo resultTypeInfo)
                        throw new Exception("Expected CLResultTypeInfo for Result CLType.");
                    var tag = _reader.ReadByte();
                    ms.WriteByte(tag);
                    // 0x01 = Ok, 0x00 = Err
                    AppendValueBytes(ms, tag == 0x01 ? resultTypeInfo.Ok : resultTypeInfo.Err);
                    break;
                }

                case CLType.Map:
                {
                    if (typeInfo is not CLMapTypeInfo mapTypeInfo)
                        throw new Exception("Expected CLMapTypeInfo for Map CLType.");
                    var countBytes = _reader.ReadBytes(4);
                    ms.Write(countBytes, 0, countBytes.Length);
                    var count = ToInt32LE(countBytes);
                    for (var i = 0; i < count; i++)
                    {
                        AppendValueBytes(ms, mapTypeInfo.KeyType);
                        AppendValueBytes(ms, mapTypeInfo.ValueType);
                    }
                    break;
                }

                case CLType.Tuple1:
                {
                    if (typeInfo is not CLTuple1TypeInfo tuple1TypeInfo)
                        throw new Exception("Expected CLTuple1TypeInfo for Tuple1 CLType.");
                    AppendValueBytes(ms, tuple1TypeInfo.Type0);
                    break;
                }

                case CLType.Tuple2:
                {
                    if (typeInfo is not CLTuple2TypeInfo tuple2TypeInfo)
                        throw new Exception("Expected CLTuple2TypeInfo for Tuple2 CLType.");
                    AppendValueBytes(ms, tuple2TypeInfo.Type0);
                    AppendValueBytes(ms, tuple2TypeInfo.Type1);
                    break;
                }

                case CLType.Tuple3:
                {
                    if (typeInfo is not CLTuple3TypeInfo tuple3TypeInfo)
                        throw new Exception("Expected CLTuple3TypeInfo for Tuple3 CLType.");
                    AppendValueBytes(ms, tuple3TypeInfo.Type0);
                    AppendValueBytes(ms, tuple3TypeInfo.Type1);
                    AppendValueBytes(ms, tuple3TypeInfo.Type2);
                    break;
                }

                default:
                    throw new Exception($"Unknown/unsupported CLType '{typeInfo.Type}'");
            }
        }

        /// <summary>
        /// Interprets four bytes as a little-endian signed 32-bit integer, regardless of host endianness.
        /// </summary>
        private static int ToInt32LE(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                var copy = new byte[4];
                Array.Copy(bytes, copy, 4);
                Array.Reverse(copy);
                return BitConverter.ToInt32(copy, 0);
            }
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}

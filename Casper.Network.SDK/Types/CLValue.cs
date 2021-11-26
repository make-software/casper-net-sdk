using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class CLValue
    {
        // Type of the value. Can be simple or constructed 
        [JsonPropertyName("cl_type")]
        [JsonConverter(typeof(CLTypeInfoConverter))]
        public CLTypeInfo TypeInfo { get; }

        // Byte array representation of underlying data
        [JsonPropertyName("bytes")]
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] Bytes { get; }

        // The optional parsed value of the bytes used when testing
        [JsonPropertyName("parsed")] public object Parsed { get; }

        public CLValue(byte[] bytes, CLType clType) :
            this(bytes, new CLTypeInfo(clType))
        {
        }

        public CLValue(byte[] bytes, CLType clType, object parsed)
            : this(bytes, new CLTypeInfo(clType), parsed)
        {
        }

        public CLValue(byte[] bytes, CLTypeInfo clType) :
            this(bytes, clType, null)
        {
        }

        public CLValue(string hexBytes, CLTypeInfo clType, object parsed)
            : this(Hex.Decode(hexBytes), clType, parsed)
        {
        }

        [JsonConstructor]
        public CLValue(byte[] bytes, CLTypeInfo typeInfo, object parsed)
        {
            TypeInfo = typeInfo;
            Bytes = bytes;

            // json deserializer may send a JsonElement
            // we can convert to string, number or null
            //
            if (parsed is JsonElement je)
            {
                Parsed = je.ValueKind switch
                {
                    JsonValueKind.String => je.GetString(),
                    JsonValueKind.Number => je.GetInt64(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => je
                };
            }
            else
                Parsed = parsed;
        }

        public static CLValue Bool(bool value)
        {
            var bytes = new byte[] {value ? (byte) 0x01 : (byte) 0x00};
            return new CLValue(bytes, new CLTypeInfo(CLType.Bool), value);
        }

        public static CLValue I32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.I32, value);
        }

        public static CLValue I64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.I64, value);
        }

        public static CLValue U8(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = value;
            return new CLValue(bytes, CLType.U8, value);
        }

        public static CLValue U32(UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.U32, value);
        }

        public static CLValue U64(UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.U64, value);
        }

        public static CLValue U128(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            var len = bytes.Length;

            byte[] b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U128, value.ToString());
        }

        public static CLValue U256(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            var len = bytes.Length;

            byte[] b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U256, value.ToString());
        }

        public static CLValue U512(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            var len = bytes.Length;

            byte[] b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U512, value.ToString());
        }

        public static CLValue U512(UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            int nonZeros;
            for (nonZeros = bytes.Length; nonZeros > 0; nonZeros--)
                if (bytes[nonZeros - 1] != 0x00)
                    break;

            byte[] b = new byte[1 + nonZeros];
            b[0] = (byte) nonZeros;
            Array.Copy(bytes, 0, b, 1, nonZeros);
            return new CLValue(b, CLType.U512, value.ToString());
        }

        public static CLValue Unit()
        {
            return new CLValue(Array.Empty<byte>(), CLType.Unit, null);
        }

        public static CLValue String(string value)
        {
            var bValue = System.Text.Encoding.UTF8.GetBytes(value);
            var bLength = BitConverter.GetBytes(bValue.Length);
            Debug.Assert(bLength.Length == 4, "It's expected that string length is encoded in 4 bytes");
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bLength);

            var bytes = new byte[4 + bValue.Length];
            Array.Copy(bLength, 0, bytes, 0, 4);
            Array.Copy(bValue, 0, bytes, 4, bValue.Length);

            return new CLValue(bytes, new CLTypeInfo(CLType.String), value);
        }

        public static CLValue URef(string value)
        {
            var uref = new URef(value);

            byte[] bytes = new byte[33];
            Array.Copy(uref.RawBytes, 0, bytes, 0, 32);
            bytes[32] = (byte) uref.AccessRights;
            return new CLValue(bytes, new CLTypeInfo(CLType.URef), value);
        }

        public static CLValue URef(URef value)
        {
            byte[] bytes = new byte[33];
            Array.Copy(value.RawBytes, 0, bytes, 0, value.RawBytes.Length);
            bytes[32] = (byte) value.AccessRights;
            return new CLValue(bytes, new CLTypeInfo(CLType.URef), value.ToString());
        }

        public static CLValue Option(CLValue innerValue)
        {
            byte[] bytes;
            if (innerValue == null)
            {
                bytes = new byte[1];
                bytes[0] = 0x00;

                return new CLValue(bytes, new CLOptionTypeInfo(null), null);
            }
            else
            {
                bytes = new byte[1 + innerValue.Bytes.Length];
                bytes[0] = 0x01;
                Array.Copy(innerValue.Bytes, 0, bytes, 1, innerValue.Bytes.Length);

                return new CLValue(bytes, new CLOptionTypeInfo(innerValue.TypeInfo), innerValue.Parsed);
            }
        }

        public static CLValue OptionNone(CLTypeInfo innerTypeInfo)
        {
            byte[] bytes = new byte[1];
            bytes[0] = 0x00;

            return new CLValue(bytes, new CLOptionTypeInfo(innerTypeInfo), null);
        }

        public static CLValue List(CLValue[] values)
        {
            if (values.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(values), "Can't create instance for empty list");

            var ms = new MemoryStream();

            byte[] bytes = BitConverter.GetBytes(values.Length);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            ms.Write(bytes);

            var typeInfo = values[0].TypeInfo;
            foreach (var clValue in values)
            {
                ms.Write(clValue.Bytes);
                if (!clValue.TypeInfo.IsListCompatibleWith(typeInfo))
                    throw new ArgumentOutOfRangeException(nameof(values), "A list cannot contain different types");
            }

            return new CLValue(ms.ToArray(), new CLListTypeInfo(typeInfo), "null");
        }

        public static CLValue ByteArray(byte[] bytes)
        {
            return new CLValue(bytes, new CLByteArrayTypeInfo(bytes.Length), Hex.ToHexString(bytes));
        }

        public static CLValue ByteArray(string hex)
        {
            var bytes = Hex.Decode(hex);
            return new CLValue(bytes, new CLByteArrayTypeInfo(bytes.Length), hex);
        }

        public static CLValue Ok(CLValue ok, CLTypeInfo errTypeInfo)
        {
            var typeInfo = new CLResultTypeInfo(ok.TypeInfo, errTypeInfo);
            var bytes = new byte[1 + ok.Bytes.Length];
            bytes[0] = 0x01;
            Array.Copy(ok.Bytes, 0, bytes, 1, ok.Bytes.Length);

            return new CLValue(bytes, typeInfo, null);
        }

        public static CLValue Err(CLValue err, CLTypeInfo okTypeInfo)
        {
            var typeInfo = new CLResultTypeInfo(okTypeInfo, err.TypeInfo);
            var bytes = new byte[1 + err.Bytes.Length];
            bytes[0] = 0x00;
            Array.Copy(err.Bytes, 0, bytes, 1, err.Bytes.Length);

            return new CLValue(bytes, typeInfo, null);
        }

        public static CLValue Map(Dictionary<CLValue, CLValue> dict)
        {
            CLMapTypeInfo mapTypeInfo = null;
            MemoryStream bytes = new MemoryStream();

            byte[] len = BitConverter.GetBytes(dict.Keys.Count);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(len);
            bytes.Write(len);

            foreach (var kv in dict)
            {
                if (mapTypeInfo == null)
                    mapTypeInfo = new CLMapTypeInfo(kv.Key.TypeInfo, kv.Value.TypeInfo);
                else if (!mapTypeInfo.KeyType.Equals(kv.Key.TypeInfo) ||
                         !mapTypeInfo.ValueType.Equals(kv.Value.TypeInfo))
                    throw new ArgumentException("All keys and values must have the same type", nameof(dict));

                bytes.Write(kv.Key.Bytes);
                bytes.Write(kv.Value.Bytes);
            }

            return new CLValue(bytes.ToArray(), mapTypeInfo, null);
        }

        public static CLValue Tuple1(CLValue t0)
        {
            return new CLValue(t0.Bytes, new CLTuple1TypeInfo(t0.TypeInfo), t0.Parsed);
        }

        public static CLValue Tuple2(CLValue t0, CLValue t1)
        {
            var bytes = new byte[t0.Bytes.Length + t1.Bytes.Length];
            Array.Copy(t0.Bytes, 0, bytes, 0, t0.Bytes.Length);
            Array.Copy(t1.Bytes, 0, bytes, t0.Bytes.Length, t1.Bytes.Length);

            return new CLValue(bytes, new CLTuple2TypeInfo(t0.TypeInfo, t1.TypeInfo), Hex.ToHexString(bytes));
        }

        public static CLValue Tuple3(CLValue t0, CLValue t1, CLValue t2)
        {
            var bytes = new byte[t0.Bytes.Length + t1.Bytes.Length + t2.Bytes.Length];
            Array.Copy(t0.Bytes, 0, bytes, 0, t0.Bytes.Length);
            Array.Copy(t1.Bytes, 0, bytes, t0.Bytes.Length, t1.Bytes.Length);
            Array.Copy(t2.Bytes, 0, bytes, t0.Bytes.Length + t1.Bytes.Length, t2.Bytes.Length);

            return new CLValue(bytes, new CLTuple3TypeInfo(t0.TypeInfo, t1.TypeInfo, t2.TypeInfo),
                Hex.ToHexString(bytes));
        }

        public static CLValue PublicKey(PublicKey publicKey)
        {
            return new CLValue(publicKey.GetBytes(), new CLTypeInfo(CLType.PublicKey),
                Hex.ToHexString(publicKey.GetBytes()));
        }

        public static CLValue PublicKey(byte[] value, KeyAlgo keyAlgorithm)
        {
            var bytes = new byte[1 + value.Length];
            bytes[0] = (byte) keyAlgorithm;
            Array.Copy(value, 0, bytes, 1, value.Length);

            return new CLValue(bytes, new CLTypeInfo(CLType.PublicKey), Hex.ToHexString(bytes));
        }

        public static CLValue PublicKey(string value, KeyAlgo keyAlgorithm)
        {
            return PublicKey(Hex.Decode(value), keyAlgorithm);
        }

        public static CLValue KeyFromPublicKey(PublicKey publicKey)
        {
            byte[] accountHash = publicKey.GetAccountHash();
            byte[] bytes = new byte[1 + accountHash.Length];
            bytes[0] = (byte) KeyIdentifier.Account;
            Array.Copy(accountHash, 0, bytes, 1, accountHash.Length);

            return new CLValue(bytes, new CLKeyTypeInfo(KeyIdentifier.Account), Hex.ToHexString(bytes));
        }

        public static CLValue Key(GlobalStateKey key)
        {
            var serializer = new GlobalStateKeyByteSerializer();

            var json = "{\"" + key.KeyIdentifier.ToString() + "\":\"" + key.ToString() + "\"}";

            return new CLValue(serializer.ToBytes(key), new CLKeyTypeInfo(key.KeyIdentifier),
                JsonDocument.Parse(json).RootElement);
        }

        #region Cast operators

        //wrap native types in CLValue with a cast operator
        //
        public static implicit operator CLValue(string s) => CLValue.String(s);

        #endregion

        #region Converter functions

        private object ReadItem(BinaryReader reader, CLType type)
        {
            return type switch
            {
                CLType.Bool => reader.ReadByte() != 0x00,
                CLType.I32 => reader.ReadInt32(),
                CLType.I64 => reader.ReadInt64(),
                CLType.U8 => reader.ReadByte(),
                CLType.U32 => reader.ReadUInt32(),
                CLType.U64 => reader.ReadUInt64(),
                CLType.U128 => reader.ReadCLBigInteger(),
                CLType.U256 => reader.ReadCLBigInteger(),
                CLType.U512 => reader.ReadCLBigInteger(),
                CLType.String => reader.ReadCLString(),
                CLType.URef => reader.ReadCLURef(),
                CLType.PublicKey => reader.ReadCLPublicKey(),
                CLType.Key => reader.ReadCLGlobalStateKey(),
                _ => null
            };
        }

        private void ParseResultError(byte[] Bytes, CLResultTypeInfo resultTypeInfo)
        {
            var reader = new BinaryReader(new MemoryStream(Bytes[1..]));

            var item = ReadItem(reader, resultTypeInfo.Err.Type);
            if (item == null)
                throw new ResultException(Bytes, resultTypeInfo.Ok.Type, resultTypeInfo.Err.Type);

            throw new ResultException(Bytes, item, resultTypeInfo.Ok.Type, resultTypeInfo.Err.Type);
        }

        private byte[] GetOkInnerTypeOrFail(ref CLTypeInfo typeInfo)
        {
            if (typeInfo is not CLResultTypeInfo)
                return Bytes;

            if (Bytes[0] != 0x01)
                ParseResultError(Bytes, (CLResultTypeInfo) typeInfo);

            typeInfo = ((CLResultTypeInfo) typeInfo).Ok;
            return Bytes[1..];
        }

        public bool ToBoolean()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.Bool)
                return bytes[0] == 0x01;

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Bool'.");
        }

        public static explicit operator bool(CLValue clValue) => clValue.ToBoolean();

        public int ToInt32()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type is CLType.I32)
                return reader.ReadCLI32();
            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Int32'.");
        }

        public static explicit operator int(CLValue clValue) => clValue.ToInt32();

        public long ToInt64()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.I64)
                return reader.ReadCLI64();
            if (typeInfo.Type == CLType.I32)
                return reader.ReadCLI32();
            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();
            if (typeInfo.Type == CLType.U32)
                return reader.ReadCLU32();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Int64'.");
        }

        public static explicit operator long(CLValue clValue) => clValue.ToInt64();

        public byte ToByte()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'byte'.");
        }

        public static explicit operator byte(CLValue clValue) => clValue.ToByte();

        public uint ToUInt32()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.U32)
                return reader.ReadCLU32();
            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'UInt32'.");
        }

        public static explicit operator uint(CLValue clValue) => clValue.ToUInt32();

        public ulong ToUInt64()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.U64)
                return reader.ReadCLU64();
            if (typeInfo.Type == CLType.U32)
                return reader.ReadCLU32();
            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'UInt64'.");
        }

        public static explicit operator ulong(CLValue clValue) => clValue.ToUInt64();

        public BigInteger ToBigInteger()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.U128 ||
                typeInfo.Type == CLType.U256 ||
                typeInfo.Type == CLType.U512)
                return reader.ReadCLBigInteger();
            if (typeInfo.Type == CLType.U64)
                return reader.ReadCLU64();
            if (typeInfo.Type == CLType.U32)
                return reader.ReadCLU32();
            if (typeInfo.Type is CLType.U8)
                return reader.ReadCLU8();
            if (typeInfo.Type == CLType.I64)
                return reader.ReadCLI64();
            if (typeInfo.Type == CLType.I32)
                return reader.ReadCLI32();

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'BigInteger'.");
        }

        public static explicit operator BigInteger(CLValue clValue) => clValue.ToBigInteger();

        public override string ToString()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            // read any type, and then, convert to string
            //
            var item = ReadItem(reader, typeInfo.Type);
            if (item == null)
                throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'String'.");

            return item.ToString();
        }

        public static explicit operator string(CLValue clValue) => clValue.ToString();

        public URef ToURef()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.URef)
                return new URef(bytes);

            if(typeInfo.Type == CLType.Key && ((CLKeyTypeInfo) typeInfo).KeyIdentifier == KeyIdentifier.URef)
                return new URef(bytes[1..]);

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'URef'.");
        }

        public static explicit operator Types.URef(CLValue clValue) => clValue.ToURef();

        public PublicKey ToPublicKey()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.PublicKey)
                return Types.PublicKey.FromBytes(bytes);

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'PublicKey'.");
        }

        public static explicit operator Types.PublicKey(CLValue clValue) => clValue.ToPublicKey();

        public GlobalStateKey ToGlobalStateKey()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.Key)
                return GlobalStateKey.FromBytes(bytes);

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'GlobalStateKey'.");
        }

        public static explicit operator Types.GlobalStateKey(CLValue clValue) => clValue.ToGlobalStateKey();

        public List<object> ToList()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.List)
            {
                var listTypeInfo = typeInfo as CLListTypeInfo;
                if (listTypeInfo == null)
                    throw new Exception("Wrong inner type in CLValue of type List");

                var length = reader.ReadCLI32();

                var list = new List<object>(length);

                for (int i = 0; i < length; i++)
                {
                    var item = ReadItem(reader, listTypeInfo.ListType.Type);
                    if (item == null)
                        throw new FormatException($"Cannot convert to a list of '{listTypeInfo.ListType.Type}'.");

                    list.Add(item);
                }

                return list;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'List'.");
        }

        public byte[] ToByteArray()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.ByteArray)
            {
                byte[] bytearray = new byte[bytes.Length];
                Array.Copy(bytes, 0, bytearray, 0, bytes.Length);
                return bytearray;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'ByteArray'.");
        }

        public static explicit operator byte[](CLValue clValue) => clValue.ToByteArray();

        public Dictionary<object, object> ToDictionary()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Map)
            {
                var mapTypeInfo = typeInfo as CLMapTypeInfo;
                if (mapTypeInfo == null)
                    throw new Exception("Wrong inner type in CLValue of type Map");

                var length = reader.ReadCLI32();

                var dict = new Dictionary<object, object>(length);

                for (int i = 0; i < length; i++)
                {
                    var key = ReadItem(reader, mapTypeInfo.KeyType.Type);
                    var value = ReadItem(reader, mapTypeInfo.ValueType.Type);
                    if (key == null || value == null)
                        throw new FormatException(
                            $"Cannot convert to a map of '{mapTypeInfo.KeyType.Type},{mapTypeInfo.ValueType.Type}'.");

                    dict.Add(key, value);
                }

                return dict;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Map'.");
        }

        #endregion
    }
}
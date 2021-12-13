using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class CLValue
    {
        /// <summary>
        /// Type of the value. Can be simple or constructed 
        /// </summary>
        [JsonPropertyName("cl_type")]
        [JsonConverter(typeof(CLTypeInfoConverter))]
        public CLTypeInfo TypeInfo { get; }

        /// <summary>
        /// Byte array representation of underlying data 
        /// </summary>
        [JsonPropertyName("bytes")]
        [JsonConverter(typeof(HexBytesWithChecksumConverter))]
        public byte[] Bytes { get; }

        /// <summary>
        /// The optional parsed value of the bytes used when testing 
        /// </summary>
        [JsonPropertyName("parsed")]
        public object Parsed { get; }

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
                    JsonValueKind.Number => typeInfo.Type switch
                    {
                        CLType.I32 => je.GetInt32(),
                        CLType.I64 => je.GetInt64(),
                        CLType.U8 => je.GetByte(),
                        CLType.U32 => je.GetUInt32(),
                        CLType.U64 => je.GetUInt64(),
                        _ => je
                    },
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => je
                };
            }
            else
                Parsed = parsed;
        }

        /// <summary>
        /// Returns a `CLValue` object with a boolean type.
        /// </summary>
        public static CLValue Bool(bool value)
        {
            var bytes = new byte[] {value ? (byte) 0x01 : (byte) 0x00};
            return new CLValue(bytes, new CLTypeInfo(CLType.Bool), value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an Int32 type.
        /// </summary>
        public static CLValue I32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.I32, value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an Int64 type.
        /// </summary>
        public static CLValue I64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.I64, value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an U8/byte type.
        /// </summary>
        public static CLValue U8(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = value;
            return new CLValue(bytes, CLType.U8, value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an UInt32 type.
        /// </summary>
        public static CLValue U32(UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.U32, value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an UInt64 type.
        /// </summary>
        public static CLValue U64(UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new CLValue(bytes, CLType.U64, value);
        }

        /// <summary>
        /// Returns a `CLValue` object with an U128 type.
        /// </summary>
        public static CLValue U128(BigInteger value)
        {
            var bytes = value.ToByteArray();
            var len = bytes.Length;

            var b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U128, value.ToString());
        }

        /// <summary>
        /// Returns a `CLValue` object with an U256 type.
        /// </summary>
        public static CLValue U256(BigInteger value)
        {
            var bytes = value.ToByteArray();
            var len = bytes.Length;

            var b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U256, value.ToString());
        }

        /// <summary>
        /// Returns a `CLValue` object with an U512 type.
        /// </summary>
        public static CLValue U512(BigInteger value)
        {
            var bytes = value.ToByteArray();
            var len = bytes.Length;

            var b = new byte[1 + len];
            b[0] = (byte) len;
            Array.Copy(bytes, 0, b, 1, len);
            return new CLValue(b, CLType.U512, value.ToString());
        }

        /// <summary>
        /// Returns a `CLValue` object with an U512 type.
        /// </summary>
        public static CLValue U512(UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            int nonZeros;
            for (nonZeros = bytes.Length; nonZeros > 0; nonZeros--)
                if (bytes[nonZeros - 1] != 0x00)
                    break;

            var b = new byte[1 + nonZeros];
            b[0] = (byte) nonZeros;
            Array.Copy(bytes, 0, b, 1, nonZeros);
            return new CLValue(b, CLType.U512, value.ToString());
        }

        /// <summary>
        /// Returns a `CLValue` object with a Unit type.
        /// </summary>
        public static CLValue Unit()
        {
            return new CLValue(Array.Empty<byte>(), CLType.Unit, null);
        }

        /// <summary>
        /// Returns a `CLValue` object with a String type.
        /// </summary>
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

        /// <summary>
        /// Returns a `CLValue` object with a URef type.
        /// </summary>
        public static CLValue URef(string value)
        {
            var uref = new URef(value);

            byte[] bytes = new byte[33];
            Array.Copy(uref.RawBytes, 0, bytes, 0, 32);
            bytes[32] = (byte) uref.AccessRights;
            return new CLValue(bytes, new CLTypeInfo(CLType.URef), value);
        }

        /// <summary>
        /// Returns a `CLValue` object with a URef type.
        /// </summary>
        public static CLValue URef(URef value)
        {
            byte[] bytes = new byte[33];
            Array.Copy(value.RawBytes, 0, bytes, 0, value.RawBytes.Length);
            bytes[32] = (byte) value.AccessRights;
            return new CLValue(bytes, new CLTypeInfo(CLType.URef), value.ToString());
        }

        /// <summary>
        /// Wraps a `CLValue` object into an Option `CLValue`.
        /// </summary>
        public static CLValue Option(CLValue innerValue)
        {
            byte[] bytes;
            bytes = new byte[1 + innerValue.Bytes.Length];
            bytes[0] = 0x01;
            Array.Copy(innerValue.Bytes, 0, bytes, 1, innerValue.Bytes.Length);

            return new CLValue(bytes, new CLOptionTypeInfo(innerValue.TypeInfo), innerValue.Parsed);
        }

        public static CLValue Option(int innerValue) => CLValue.Option(CLValue.I32(innerValue));

        public static CLValue Option(long innerValue) => CLValue.Option(CLValue.I64(innerValue));

        public static CLValue Option(byte innerValue) => CLValue.Option(CLValue.U8(innerValue));

        public static CLValue Option(uint innerValue) => CLValue.Option(CLValue.U32(innerValue));

        public static CLValue Option(ulong innerValue) => CLValue.Option(CLValue.U64(innerValue));

        public static CLValue Option(string innerValue) => CLValue.Option(CLValue.String(innerValue));

        public static CLValue Option(URef innerValue) => CLValue.Option(CLValue.URef(innerValue));

        public static CLValue Option(PublicKey innerValue) => CLValue.Option(CLValue.PublicKey(innerValue));

        public static CLValue Option(GlobalStateKey innerValue) => CLValue.Option(CLValue.Key(innerValue));

        public static CLValue Option(byte[] innerValue) => CLValue.Option(CLValue.ByteArray(innerValue));

        public static CLValue Option(CLValue[] innerValue) => CLValue.Option(CLValue.List(innerValue));

        public static CLValue Option(Dictionary<CLValue, CLValue> innerValue) =>
            CLValue.Option(CLValue.Map(innerValue));

        public static CLValue OptionNone(CLTypeInfo innerTypeInfo)
        {
            byte[] bytes = new byte[1];
            bytes[0] = 0x00;

            return new CLValue(bytes, new CLOptionTypeInfo(innerTypeInfo), null);
        }

        /// <summary>
        /// Returns a List `CLValue` object.
        /// </summary>
        public static CLValue List(CLValue[] values)
        {
            if (values.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(values), "Can't create instance for empty list");

            var ms = new MemoryStream();

            var bytes = BitConverter.GetBytes(values.Length);
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

            return new CLValue(ms.ToArray(), new CLListTypeInfo(typeInfo), "");
        }

        /// <summary>
        /// Returns a `CLValue` object with a ByteArray type.
        /// </summary>
        public static CLValue ByteArray(byte[] bytes)
        {
            return new CLValue(bytes, new CLByteArrayTypeInfo(bytes.Length), Hex.ToHexString(bytes));
        }

        /// <summary>
        /// Returns a `CLValue` object with a ByteArray type.
        /// </summary>
        public static CLValue ByteArray(string hex)
        {
            var bytes = Hex.Decode(hex);
            return new CLValue(bytes, new CLByteArrayTypeInfo(bytes.Length), hex);
        }

        /// <summary>
        /// Returns a Result `CLValue` with wrapped OK value inside.
        /// To be complete, it must be indicated the type for an err value
        /// </summary>
        public static CLValue Ok(CLValue ok, CLTypeInfo errTypeInfo)
        {
            var typeInfo = new CLResultTypeInfo(ok.TypeInfo, errTypeInfo);
            var bytes = new byte[1 + ok.Bytes.Length];
            bytes[0] = 0x01;
            Array.Copy(ok.Bytes, 0, bytes, 1, ok.Bytes.Length);

            return new CLValue(bytes, typeInfo, null);
        }

        /// <summary>
        /// Returns a Result `CLValue` with wrapped Err value inside.
        /// To be complete, it must be indicated the type for an ok value
        /// </summary>
        public static CLValue Err(CLValue err, CLTypeInfo okTypeInfo)
        {
            var typeInfo = new CLResultTypeInfo(okTypeInfo, err.TypeInfo);
            var bytes = new byte[1 + err.Bytes.Length];
            bytes[0] = 0x00;
            Array.Copy(err.Bytes, 0, bytes, 1, err.Bytes.Length);

            return new CLValue(bytes, typeInfo, null);
        }

        /// <summary>
        /// Returns a Map `CLValue` object.
        /// </summary>
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

        /// <summary>
        /// Returns a Tuple1 `CLValue` object.
        /// </summary>
        public static CLValue Tuple1(CLValue t0)
        {
            return new CLValue(t0.Bytes, new CLTuple1TypeInfo(t0.TypeInfo), t0.Parsed);
        }

        /// <summary>
        /// Returns a Tuple2 `CLValue` object.
        /// </summary>
        public static CLValue Tuple2(CLValue t0, CLValue t1)
        {
            var bytes = new byte[t0.Bytes.Length + t1.Bytes.Length];
            Array.Copy(t0.Bytes, 0, bytes, 0, t0.Bytes.Length);
            Array.Copy(t1.Bytes, 0, bytes, t0.Bytes.Length, t1.Bytes.Length);

            return new CLValue(bytes, new CLTuple2TypeInfo(t0.TypeInfo, t1.TypeInfo), Hex.ToHexString(bytes));
        }

        /// <summary>
        /// Returns a Tuple3 `CLValue` object.
        /// </summary>
        public static CLValue Tuple3(CLValue t0, CLValue t1, CLValue t2)
        {
            var bytes = new byte[t0.Bytes.Length + t1.Bytes.Length + t2.Bytes.Length];
            Array.Copy(t0.Bytes, 0, bytes, 0, t0.Bytes.Length);
            Array.Copy(t1.Bytes, 0, bytes, t0.Bytes.Length, t1.Bytes.Length);
            Array.Copy(t2.Bytes, 0, bytes, t0.Bytes.Length + t1.Bytes.Length, t2.Bytes.Length);

            return new CLValue(bytes, new CLTuple3TypeInfo(t0.TypeInfo, t1.TypeInfo, t2.TypeInfo),
                Hex.ToHexString(bytes));
        }

        /// <summary>
        /// Returns a `CLValue` object with a PublicKey type.
        /// </summary>
        public static CLValue PublicKey(PublicKey publicKey)
        {
            return new CLValue(publicKey.GetBytes(), new CLTypeInfo(CLType.PublicKey),
                Hex.ToHexString(publicKey.GetBytes()));
        }

        /// <summary>
        /// Returns a `CLValue` object with a PublicKey type.
        /// </summary>
        public static CLValue PublicKey(byte[] value, KeyAlgo keyAlgorithm)
        {
            var bytes = new byte[1 + value.Length];
            bytes[0] = (byte) keyAlgorithm;
            Array.Copy(value, 0, bytes, 1, value.Length);

            return new CLValue(bytes, new CLTypeInfo(CLType.PublicKey), Hex.ToHexString(bytes));
        }

        /// <summary>
        /// Converts a public key into an account hash an returns it wrapped into a Key `CLValue`
        /// </summary>
        public static CLValue KeyFromPublicKey(PublicKey publicKey)
        {
            byte[] accountHash = new AccountHashKey(publicKey.GetAccountHash()).RawBytes;
            byte[] bytes = new byte[1 + accountHash.Length];
            bytes[0] = (byte) KeyIdentifier.Account;
            Array.Copy(accountHash, 0, bytes, 1, accountHash.Length);

            return new CLValue(bytes, new CLKeyTypeInfo(KeyIdentifier.Account), Hex.ToHexString(bytes));
        }

        /// <summary>
        /// Returns a `CLValue` object with a GlobalStateKey in it 
        /// </summary>
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

        private void ParseResultError(byte[] Bytes, CLResultTypeInfo resultTypeInfo)
        {
            var reader = new BinaryReader(new MemoryStream(Bytes[1..]));

            var item = reader.ReadCLItem(resultTypeInfo.Err, null);
            if (item == null)
                throw new CLValueException(Bytes, resultTypeInfo.Ok, resultTypeInfo.Err);

            throw new CLValueException(Bytes, item, resultTypeInfo.Ok, resultTypeInfo.Err);
        }

        private byte[] GetOkInnerTypeOrFail(ref CLTypeInfo typeInfo)
        {
            if (typeInfo is not CLResultTypeInfo resultTypeInfo)
                return Bytes;

            if (Bytes[0] != 0x01)
                ParseResultError(Bytes, resultTypeInfo);

            typeInfo = resultTypeInfo.Ok;
            return Bytes[1..];
        }

        /// <summary>
        /// Converts a `CLValue`to a boolean 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a Int32 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a Int64 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a U8/byte 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a UInt32 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a UInt64 
        /// </summary>
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

        /// <summary>
        /// Converts a `CLValue`to a BigInteger 
        /// </summary>
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

        /// <summary>
        /// Converts Unit CLValue to Unit.
        /// </summary>
        public Unit ToUnit()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.Unit)
                return Types.Unit.Default;

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Unit'.");
        }
        
        public static explicit operator Unit(CLValue clValue) => clValue.ToUnit();

        
        /// <summary>
        /// Converts a `CLValue`to a String 
        /// </summary>
        public override string ToString()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            // read any type, and then, convert to string
            //
            var item = reader.ReadCLItem(typeInfo, null);
            if (item == null)
                throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'String'.");

            return item.ToString();
        }

        public static explicit operator string(CLValue clValue) => clValue.ToString();

        /// <summary>
        /// Converts a `CLValue`to a URef 
        /// </summary>
        public URef ToURef()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.URef)
                return new URef(bytes);

            if (typeInfo.Type == CLType.Key && ((CLKeyTypeInfo) typeInfo).KeyIdentifier == KeyIdentifier.URef)
                return new URef(bytes[1..]);

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'URef'.");
        }

        public static explicit operator URef(CLValue clValue) => clValue.ToURef();

        /// <summary>
        /// Converts a `CLValue`to a PublicKey 
        /// </summary>
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

        public static explicit operator PublicKey(CLValue clValue) => clValue.ToPublicKey();

        /// <summary>
        /// Converts a `CLValue` to a GlobalStateKey 
        /// </summary>
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

        public static explicit operator GlobalStateKey(CLValue clValue) => clValue.ToGlobalStateKey();

        /// <summary>
        /// Converts a List `CLValue` to a List&lt;&gt;.
        /// </summary>
        public IList ToList()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.List)
            {
                var dict = reader.ReadCLItem(typeInfo, null);
                return (IList) dict;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'List'.");
        }

        /// <summary>
        /// Converts a List `CLValue` to a List&lt;T&gt;.
        /// </summary>
        public List<T> ToList<T>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.List)
            {
                var fwType = typeof(List<>).MakeGenericType(new[] {typeof(T)});
                var dict = reader.ReadCLItem(typeInfo, fwType);
                return (List<T>) dict;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'List'.");
        }

        /// <summary>
        /// Converts ByteArray `CLValue` to a byte[] 
        /// </summary>
        public byte[] ToByteArray()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            if (typeInfo.Type == CLType.ByteArray)
            {
                var reader = new BinaryReader(new MemoryStream(bytes));

                return (byte[]) reader.ReadCLItem(typeInfo, typeof(byte[]));
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'ByteArray'.");
        }

        public static explicit operator byte[](CLValue clValue) => clValue.ToByteArray();

        /// <summary>
        /// Converts Map CLValue to Dictionary&lt;CLValue,CLValue&gt;.
        /// </summary>
        public IDictionary ToDictionary()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Map)
            {
                return (IDictionary) reader.ReadCLItem(typeInfo, null);
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Map'.");
        }

        /// <summary>
        /// Converts Map CLValue to Dictionary&lt;TKey,TValue&gt;.
        /// </summary>
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Map)
            {
                var fwType = typeof(Dictionary<,>).MakeGenericType(new[] {typeof(TKey), typeof(TValue)});
                var dict = reader.ReadCLItem(typeInfo, fwType);
                return (Dictionary<TKey, TValue>) dict;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Map'.");
        }

        /// <summary>
        /// Converts Tuple1 CLValue to Tuple&ltT1;&gt;.
        /// </summary>
        public Tuple<T1> ToTuple1<T1>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Tuple1)
            {
                var fwType = typeof(Tuple<>).MakeGenericType(new[] {typeof(T1)});
                var tuple = reader.ReadCLItem(typeInfo, fwType);
                return (Tuple<T1>) tuple;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Tuple'.");
        }

        /// <summary>
        /// Converts Tuple2 CLValue to Tuple&ltT1,T2;&gt;.
        /// </summary>
        public Tuple<T1, T2> ToTuple2<T1, T2>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Tuple2)
            {
                var fwType = typeof(Tuple<,>).MakeGenericType(new[] {typeof(T1), typeof(T2)});
                var tuple = reader.ReadCLItem(typeInfo, fwType);
                return (Tuple<T1,T2>) tuple;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Tuple'.");
        }

        /// <summary>
        /// Converts Tuple2 CLValue to Tuple&ltT1,T2,T3;&gt;.
        /// </summary>
        public Tuple<T1, T2, T3> ToTuple3<T1, T2, T3>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type == CLType.Result)
                bytes = GetOkInnerTypeOrFail(ref typeInfo);

            var reader = new BinaryReader(new MemoryStream(bytes));

            if (typeInfo.Type == CLType.Tuple3)
            {
                var fwType = typeof(Tuple<,,>).MakeGenericType(new[] {typeof(T1), typeof(T2), typeof(T3)});
                var tuple = reader.ReadCLItem(typeInfo, fwType);
                return (Tuple<T1,T2,T3>) tuple;
            }

            throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Tuple'.");
        }

        /// <summary>
        /// Converts Result CLValue to Result&ltTOk,TErr;&gt;.
        /// </summary>
        public Result<TOk, TErr> ToResult<TOk, TErr>()
        {
            var bytes = Bytes;
            var typeInfo = TypeInfo;

            if (typeInfo.Type != CLType.Result)
                throw new FormatException($"Cannot convert '{typeInfo.Type}' to 'Result'.");

            if (typeInfo is not CLResultTypeInfo resultTypeInfo)
                throw new FormatException($"Cannot convert '{typeInfo.GetType()}' to 'CLResultTypeInfo'.");

            if (Bytes[0] == 0x01)
            {
                var reader = new BinaryReader(new MemoryStream(Bytes[1..]));
                var v = reader.ReadCLItem(resultTypeInfo.Ok, typeof(TOk));
                return Result<TOk, TErr>.Ok((TOk) v);
            }
            else
            {
                var reader = new BinaryReader(new MemoryStream(Bytes[1..]));
                var v = reader.ReadCLItem(resultTypeInfo.Err, typeof(TErr));
                return Result<TOk, TErr>.Fail((TErr)v);
            }
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Utils
{
    public static class BinaryReaderExtensions
    {
        public static int ReadCLI32(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes);
        }

        public static long ReadCLI64(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(8);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes);
        }

        public static byte ReadCLU8(this BinaryReader reader)
        {
            return reader.ReadByte();
        }

        public static uint ReadCLU32(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes);
        }

        public static ulong ReadCLU64(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(8);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        public static BigInteger ReadCLBigInteger(this BinaryReader reader)
        {
            var length = (int) reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            return new BigInteger(bytes, true, false);
        }

        public static string ReadCLString(this BinaryReader reader)
        {
            var length = (int) reader.ReadInt32();
            var bytes = reader.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static URef ReadCLURef(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(33);
            return new URef(bytes);
        }

        public static PublicKey ReadCLPublicKey(this BinaryReader reader)
        {
            int keyAlgo = reader.PeekChar();
            return PublicKey.FromBytes(reader.ReadBytes(keyAlgo == 0x01
                ? KeyAlgo.ED25519.GetKeySizeInBytes()
                : KeyAlgo.SECP256K1.GetKeySizeInBytes()));
        }

        public static GlobalStateKey ReadCLGlobalStateKey(this BinaryReader reader)
        {
            int keyId = reader.PeekChar();

            // Era Info serializes as a u64 (8 bytes + 1 tag byte)
            if (keyId == (char) KeyIdentifier.EraInfo)
                return GlobalStateKey.FromBytes(reader.ReadBytes(9));

            //URef serializes as 33 bytes + 1 tag byte
            if (keyId == (char) KeyIdentifier.URef)
                return GlobalStateKey.FromBytes(reader.ReadBytes(34));

            // all others serialize as 32 bytes + 1 tag byte
            return GlobalStateKey.FromBytes(reader.ReadBytes(33));
        }

        public static byte[] ReadCLByteArray(this BinaryReader reader, CLTypeInfo typeInfo)
        {
            if (typeInfo is not CLByteArrayTypeInfo baTypeInfo)
                throw new Exception("Wrong type info class. Expected CLByteArrayTypeInfo, Received " +
                                    typeInfo.GetType());

            var bytes = new byte[baTypeInfo.Size];
            reader.Read(bytes, 0, baTypeInfo.Size);

            return bytes;
        }

        public static IList ReadCLList(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            IList list;
            Type valueType = null;

            if (typeInfo is not CLListTypeInfo listTypeInfo)
                throw new Exception("Wrong type info class. Expected CLListTypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                valueType = tArgs[0];

                list = Activator.CreateInstance(fwType) as IList;
                if (list == null)
                    throw new Exception($"Cannot create a list with type '{fwType}'.");
            }
            else
            {
                valueType = listTypeInfo.ListType.GetFrameworkType();
                
                var listType = typeof(List<>).MakeGenericType(new[] {valueType});
                list  = Activator.CreateInstance(listType) as IList;
                if (list == null)
                    throw new Exception($"Cannot create a list with type '{listType}'.");
            }

            int length = reader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                var item = ReadCLItem(reader, listTypeInfo.ListType, valueType);
                list.Add(item);
            }

            return list;
        }

        public static IDictionary ReadCLMap(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            IDictionary dict;
            Type keyType = null;
            Type valueType = null;

            if (typeInfo is not CLMapTypeInfo mapTypeInfo)
                throw new Exception("Wrong type info class. Expected CLMapTypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                if (tArgs.Length == 2)
                {
                    keyType = tArgs[0];
                    valueType = tArgs[1];    
                }

                dict = Activator.CreateInstance(fwType) as IDictionary;
                if (dict == null)
                    throw new Exception($"Cannot create a dictionary with type '{fwType}'.");
            }
            else
            {
                keyType = mapTypeInfo.KeyType.GetFrameworkType();
                valueType = mapTypeInfo.ValueType.GetFrameworkType();
                
                var dictType = typeof(Dictionary<,>).MakeGenericType(new[] {keyType, valueType});
                dict  = Activator.CreateInstance(dictType) as IDictionary;
                if (dict == null)
                    throw new Exception($"Cannot create a dictionary with type '{dict}'.");
            }

            int length = reader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                var key = ReadCLItem(reader, mapTypeInfo.KeyType, keyType);
                var value = ReadCLItem(reader, mapTypeInfo.ValueType, valueType);

                dict.Add(key, value);
            }

            return dict;
        }

        public static ITuple ReadCLTuple1(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            Type t0Type = null;

            if (typeInfo is not CLTuple1TypeInfo tupleTypeInfo)
                throw new Exception("Wrong type info class. Expected CLTuple1TypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                t0Type = tArgs[0];
            }
            else
            {
                t0Type = tupleTypeInfo.Type0.GetFrameworkType();
            }

            var t0 = ReadCLItem(reader, tupleTypeInfo.Type0, t0Type);

            var tupleType = typeof(Tuple<>).MakeGenericType(new[] {t0Type});
            var tuple = Activator.CreateInstance(tupleType, t0);
            
            return (ITuple)tuple;
        }

        public static ITuple ReadCLTuple2(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            Type t0Type = null;
            Type t1Type = null;

            if (typeInfo is not CLTuple2TypeInfo tupleTypeInfo)
                throw new Exception("Wrong type info class. Expected CLTuple2TypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                t0Type = tArgs[0];
                t1Type = tArgs[1];
            }
            else
            {
                t0Type = tupleTypeInfo.Type0.GetFrameworkType();
                t1Type = tupleTypeInfo.Type1.GetFrameworkType();
            }

            var t0 = ReadCLItem(reader, tupleTypeInfo.Type0, t0Type);
            var t1 = ReadCLItem(reader, tupleTypeInfo.Type1, t1Type);

            var tupleType = typeof(Tuple<,>).MakeGenericType(new[] {t0Type, t1Type});
            var tuple = Activator.CreateInstance(tupleType, t0, t1);
            
            return (ITuple)tuple;
        }

        public static ITuple ReadCLTuple3(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            Type t0Type = null;
            Type t1Type = null;
            Type t2Type = null;

            if (typeInfo is not CLTuple3TypeInfo tupleTypeInfo)
                throw new Exception("Wrong type info class. Expected CLTuple3TypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                t0Type = tArgs[0];
                t1Type = tArgs[1];
                t2Type = tArgs[2];
            }
            else
            {
                t0Type = tupleTypeInfo.Type0.GetFrameworkType();
                t1Type = tupleTypeInfo.Type1.GetFrameworkType();
                t2Type = tupleTypeInfo.Type2.GetFrameworkType();
            }

            var t0 = ReadCLItem(reader, tupleTypeInfo.Type0, t0Type);
            var t1 = ReadCLItem(reader, tupleTypeInfo.Type1, t1Type);
            var t2 = ReadCLItem(reader, tupleTypeInfo.Type2, t2Type);

            var tupleType = typeof(Tuple<,,>).MakeGenericType(new[] {t0Type, t1Type, t2Type});
            var tuple = Activator.CreateInstance(tupleType, t0, t1, t2);
            
            return (ITuple)tuple;
        }

        public static object ReadCLOption(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            Type optType = null;
            
            if (typeInfo is not CLOptionTypeInfo optionTypeInfo)
                throw new Exception("Wrong type info class. Expected CLOptionTypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                optType = fwType;
            }
            else
            {
                // extract from CLTypeInfo
                //
                optType = optionTypeInfo.OptionType.GetFrameworkType();
            }

            var opt = reader.ReadByte();

            return opt == 0x00 ? null : ReadCLItem(reader, optionTypeInfo.OptionType, optType);
        }

        public static object ReadCLResult(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType = null)
        {
            Type okType = null;
            Type errType = null;

            if (typeInfo is not CLResultTypeInfo resultTypeInfo)
                throw new Exception("Wrong type info class. Expected CLResultTypeInfo, Received " + typeInfo.GetType());

            if (fwType != null && !fwType.IsInterface)
            {
                var tArgs = fwType.GetGenericArguments();
                okType = tArgs[0];
                errType = tArgs[1];
            }
            else
            {
                // extract from CLTypeInfo
                //
                okType = resultTypeInfo.Ok.GetFrameworkType();
                errType = resultTypeInfo.Err.GetFrameworkType();
            }

            var resType = typeof(Result<,>).MakeGenericType(new[] {okType, errType});

            var res = reader.ReadByte();
            if (res == 0x01)
            {
                var item = ReadCLItem(reader, resultTypeInfo.Ok, okType);
                var okValue = Convert.ChangeType(item, okType);

                var method = resType.GetMethod("Ok");
                if (method == null)
                    throw new InvalidOperationException("Cannot retrieve method 'Ok' from Result class");

                var result = method.Invoke(null, new object[] {okValue});

                return result;
            }
            else
            {
                var item = ReadCLItem(reader, resultTypeInfo.Err, errType);
                var errValue = Convert.ChangeType(item, errType);

                var method = resType.GetMethod("Fail");
                if (method == null)
                    throw new InvalidOperationException("Cannot retrieve method 'Fail' from Result class");
                var result = method.Invoke(null, new object[] {errValue});

                return result;
            }
        }

        public static object ReadCLItem(this BinaryReader reader, CLTypeInfo typeInfo, Type fwType)
        {
            return typeInfo.Type switch
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
                CLType.Unit => Unit.Default,
                CLType.String => reader.ReadCLString(),
                CLType.URef => reader.ReadCLURef(),
                CLType.PublicKey => reader.ReadCLPublicKey(),
                CLType.Key => reader.ReadCLGlobalStateKey(),
                CLType.List => reader.ReadCLList(typeInfo, fwType),
                CLType.ByteArray => reader.ReadCLByteArray(typeInfo),
                CLType.Map => reader.ReadCLMap(typeInfo, fwType),
                CLType.Tuple1 => reader.ReadCLTuple1(typeInfo, fwType),
                CLType.Tuple2 => reader.ReadCLTuple2(typeInfo, fwType),
                CLType.Tuple3 => reader.ReadCLTuple3(typeInfo, fwType),
                CLType.Option => reader.ReadCLOption(typeInfo, fwType),
                CLType.Result => reader.ReadCLResult(typeInfo, fwType),
                _ => throw new Exception($"Unknown/unsupported CLType '{typeInfo.Type}'")
            };
        }

        public static TItem ReadCLItem<TItem>(BinaryReader reader, CLTypeInfo typeInfo)
        {
            var item = reader.ReadCLItem(typeInfo, typeof(TItem));

            return (TItem) item;
        }
    }
}
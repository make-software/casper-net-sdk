using System;
using System.Collections.Generic;
using System.IO;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.CES
{
    /// <summary>
    /// Parses CES (Casper Event Standard) schema and event bytes produced by
    /// smart contracts following the make-software CES convention.
    /// <para>
    /// Contracts store their event schema in the named key <c>__events_schema</c>
    /// (a <c>CLValue</c> of type <c>Any</c>) and emit events into <c>__events</c>
    /// (a <c>CLValue</c> of type <c>Map(U32, Bytes)</c>).
    /// </para>
    /// </summary>
    public static class CESParser
    {
        /// <summary>
        /// Parses the raw bytes of the <c>__events_schema</c> CLValue (type <c>Any</c>)
        /// into a <see cref="CESContractSchema"/>.
        /// </summary>
        /// <param name="rawBytes">
        /// The <c>Bytes</c> property of the CLValue retrieved from <c>__events_schema</c>.
        /// </param>
        public static CESContractSchema ParseSchema(byte[] rawBytes)
        {
            using var ms = new MemoryStream(rawBytes);
            using var reader = new BinaryReader(ms);

            var numEvents = reader.ReadInt32();
            var events = new Dictionary<string, CESEventSchema>(numEvents);

            for (int i = 0; i < numEvents; i++)
            {
                var eventName = reader.ReadCLString();

                var numFields = reader.ReadInt32();
                var fields = new List<CESEventSchemaField>(numFields);

                for (int j = 0; j < numFields; j++)
                {
                    var fieldName = reader.ReadCLString();
                    var fieldType = ReadCLTypeInfo(reader);
                    fields.Add(new CESEventSchemaField(fieldName, fieldType));
                }

                events[eventName] = new CESEventSchema(eventName, fields);
            }

            return new CESContractSchema(events);
        }

        /// <summary>
        /// Parses a single CES event from its raw bytes using the given contract schema.
        /// </summary>
        /// <param name="rawBytes">
        /// The raw bytes of one event entry (value) from the <c>__events</c> CLValue map.
        /// </param>
        /// <param name="schema">
        /// The contract schema obtained from <see cref="ParseSchema"/>.
        /// </param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the event name found in <paramref name="rawBytes"/> is not present in
        /// <paramref name="schema"/>.
        /// </exception>
        public static CESEvent ParseEvent(byte[] rawBytes, CESContractSchema schema)
        {
            // Some CES implementations wrap the event payload in a Casper Vec<u8> (Bytes),
            // which prepends a u32 LE length equal to the remaining byte count. Detect and
            // skip that outer wrapper transparently.
            var offset = 0;
            if (rawBytes.Length >= 4)
            {
                var declaredLen = BitConverter.ToUInt32(rawBytes, 0);
                if (!BitConverter.IsLittleEndian)
                    declaredLen = SwapU32(declaredLen);
                if (declaredLen == rawBytes.Length - 4)
                    offset = 4;
            }

            using var ms = new MemoryStream(rawBytes, offset, rawBytes.Length - offset);
            using var reader = new BinaryReader(ms);

            var rawName = reader.ReadCLString();

            // Strip the "event_" prefix that some CES implementations prepend to event names.
            const string prefix = "event_";
            var eventName = rawName.StartsWith(prefix)
                ? rawName.Substring(prefix.Length)
                : rawName;

            if (!schema.TryGetEventSchema(eventName, out var eventSchema))
                throw new KeyNotFoundException(
                    $"Event '{rawName}' not found in the contract schema.");

            var fields = new List<NamedArg>(eventSchema.Fields.Count);

            foreach (var schemaField in eventSchema.Fields)
            {
                var fwType = schemaField.CLTypeInfo.GetFrameworkType();
                var value = reader.ReadCLItem(schemaField.CLTypeInfo, fwType);
                var clValue = BuildCLValue(value, schemaField.CLTypeInfo);
                fields.Add(new NamedArg(schemaField.Name, clValue));
            }

            return new CESEvent(rawName, fields);
        }

        private static uint SwapU32(uint value) =>
            ((value & 0x000000FFu) << 24) |
            ((value & 0x0000FF00u) <<  8) |
            ((value & 0x00FF0000u) >>  8) |
            ((value & 0xFF000000u) >> 24);

        /// <summary>
        /// Reads a <see cref="CLTypeInfo"/> from the binary stream.
        /// Mirrors <c>CLValueByteSerializer.CLTypeToBytes()</c> in reverse.
        /// </summary>
        private static CLTypeInfo ReadCLTypeInfo(BinaryReader reader)
        {
            var tag = (CLType)reader.ReadByte();

            return tag switch
            {
                CLType.Option    => new CLOptionTypeInfo(ReadCLTypeInfo(reader)),
                CLType.List      => new CLListTypeInfo(ReadCLTypeInfo(reader)),
                CLType.ByteArray => new CLByteArrayTypeInfo(reader.ReadInt32()),
                CLType.Result    => new CLResultTypeInfo(ReadCLTypeInfo(reader), ReadCLTypeInfo(reader)),
                CLType.Map       => new CLMapTypeInfo(ReadCLTypeInfo(reader), ReadCLTypeInfo(reader)),
                CLType.Tuple1    => new CLTuple1TypeInfo(ReadCLTypeInfo(reader)),
                CLType.Tuple2    => new CLTuple2TypeInfo(ReadCLTypeInfo(reader), ReadCLTypeInfo(reader)),
                CLType.Tuple3    => new CLTuple3TypeInfo(ReadCLTypeInfo(reader), ReadCLTypeInfo(reader), ReadCLTypeInfo(reader)),
                _                => new CLTypeInfo(tag)
            };
        }

        /// <summary>
        /// Wraps a deserialized field value back into a <see cref="CLValue"/> so that callers
        /// can use the standard <c>CLValue.ToXxx()</c> conversion methods on each field.
        /// </summary>
        private static CLValue BuildCLValue(object value, CLTypeInfo typeInfo)
        {
            return typeInfo.Type switch
            {
                CLType.Bool      => CLValue.Bool((bool)value),
                CLType.I32       => CLValue.I32((int)value),
                CLType.I64       => CLValue.I64((long)value),
                CLType.U8        => CLValue.U8((byte)value),
                CLType.U32       => CLValue.U32((uint)value),
                CLType.U64       => CLValue.U64((ulong)value),
                CLType.U128      => CLValue.U128((System.Numerics.BigInteger)value),
                CLType.U256      => CLValue.U256((System.Numerics.BigInteger)value),
                CLType.U512      => CLValue.U512((System.Numerics.BigInteger)value),
                CLType.Unit      => CLValue.Unit(),
                CLType.String    => CLValue.String((string)value),
                CLType.URef      => CLValue.URef((URef)value),
                CLType.PublicKey => CLValue.PublicKey((PublicKey)value),
                CLType.Key       => CLValue.Key((GlobalStateKey)value),
                _                => new CLValue(SerializeValue(value, typeInfo), typeInfo, value)
            };
        }

        /// <summary>
        /// Re-serializes a complex CLValue (Option, List, Map, etc.) to bytes so it can be
        /// stored inside a <see cref="CLValue"/> wrapper.  Uses the SDK's existing serializer.
        /// </summary>
        private static byte[] SerializeValue(object value, CLTypeInfo typeInfo)
        {
            var serializer = new ByteSerializers.CLValueByteSerializer();
            // Build a temporary CLValue to get its inner bytes via the serializer.
            // For complex types we need the raw data bytes, which are the first part of
            // the full serialization (data_length + data_bytes + type_bytes).
            using var ms = new MemoryStream();

            switch (typeInfo)
            {
                case CLOptionTypeInfo optionType:
                {
                    if (value == null)
                    {
                        ms.WriteByte(0x00);
                    }
                    else
                    {
                        ms.WriteByte(0x01);
                        var inner = BuildCLValue(value, optionType.OptionType);
                        ms.Write(inner.Bytes);
                    }
                    break;
                }
                case CLListTypeInfo listType:
                {
                    var list = (System.Collections.IList)value;
                    var lenBytes = BitConverter.GetBytes(list.Count);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                    ms.Write(lenBytes);
                    foreach (var item in list)
                    {
                        var inner = BuildCLValue(item, listType.ListType);
                        ms.Write(inner.Bytes);
                    }
                    break;
                }
                case CLMapTypeInfo mapType:
                {
                    var dict = (System.Collections.IDictionary)value;
                    var lenBytes = BitConverter.GetBytes(dict.Count);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                    ms.Write(lenBytes);
                    foreach (System.Collections.DictionaryEntry kv in dict)
                    {
                        var k = BuildCLValue(kv.Key, mapType.KeyType);
                        var v = BuildCLValue(kv.Value, mapType.ValueType);
                        ms.Write(k.Bytes);
                        ms.Write(v.Bytes);
                    }
                    break;
                }
                case CLByteArrayTypeInfo baType:
                {
                    ms.Write((byte[])value);
                    break;
                }
                default:
                    throw new NotSupportedException(
                        $"SerializeValue not implemented for CLType '{typeInfo.Type}'.");
            }

            return ms.ToArray();
        }
    }
}

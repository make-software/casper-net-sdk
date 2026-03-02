using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Named arguments passed as input in a Deploy item. 
    /// </summary>
    public class NamedArg
    {
        public string Name { get; }

        public CLValue Value { get; }

        /// <summary>
        /// Creates a NamedArg object with a `CLValue` value. 
        /// </summary>
        public NamedArg(string name, CLValue value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Creates a NamedArg object with a Boolean value. 
        /// </summary>
        public NamedArg(string name, bool value) : this(name, CLValue.Bool(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with an Int32 value. 
        /// </summary>
        public NamedArg(string name, int value) : this(name, CLValue.I32(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with an Int64 value. 
        /// </summary>
        public NamedArg(string name, long value) : this(name, CLValue.I64(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with an U8/byte value. 
        /// </summary>
        public NamedArg(string name, byte value) : this(name, CLValue.U8(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with an UInt32 value. 
        /// </summary>
        public NamedArg(string name, uint value) : this(name, CLValue.U32(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with an UInt64 value. 
        /// </summary>
        public NamedArg(string name, ulong value) : this(name, CLValue.U64(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a BigInteger value. 
        /// </summary>
        public NamedArg(string name, BigInteger value) : this(name, CLValue.U512(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a string value. 
        /// </summary>
        public NamedArg(string name, string value) : this(name, CLValue.String(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a URef value. 
        /// </summary>
        public NamedArg(string name, URef value) : this(name, CLValue.URef(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a byte array value. 
        /// </summary>
        public NamedArg(string name, byte[] value) : this(name, CLValue.ByteArray(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a PublicKey value. 
        /// </summary>
        public NamedArg(string name, PublicKey value) : this(name, CLValue.PublicKey(value))
        {
        }

        /// <summary>
        /// Creates a NamedArg object with a GlobalStateKey value. 
        /// </summary>
        public NamedArg(string name, GlobalStateKey value) : this(name, CLValue.Key(value))
        {
        }

        public class NamedArgConverter : JsonConverter<NamedArg>
        {
            public override NamedArg Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                reader.Read(); // start array
                var name = reader.GetString();
                reader.Read();
                CLValue clValue = null;
                if (reader.TokenType == JsonTokenType.StartObject)
                    clValue = JsonSerializer.Deserialize<CLValue>(ref reader, options);
                reader.Read(); // end object
                return new NamedArg(name, clValue);
            }

            public override void Write(
                Utf8JsonWriter writer,
                NamedArg namedArg,
                JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(namedArg.Name);
                JsonSerializer.Serialize(writer, namedArg.Value, options);
                writer.WriteEndArray();
            }
        }

        public static IList<NamedArg> ListFromBytes(byte[] bytes)
        {
            //create a memory stream from the bytes
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            return ListFromReader(reader);
        }

        public static IList<NamedArg> ListFromReader(BinaryReader reader)
        {
            // read the number of named args
            var numNamedArgs = reader.ReadInt32();

            var namedArgs = new List<NamedArg>(numNamedArgs);

            for (int i = 0; i < numNamedArgs; i++)
            {
                var namedArg = new NamedArgByteSerializer().FromReader(reader);
                namedArgs.Add(namedArg);
            }

            return namedArgs;
        }
    }
}
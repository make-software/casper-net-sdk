using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    public class NamedArg
    {
        public string Name { get; }
        public CLValue Value { get; }

        public NamedArg(string name, CLValue value)
        {
            Name = name;
            Value = value;
        }

        public NamedArg(string name, bool value) : this(name, CLValue.Bool(value))
        {
        }

        public NamedArg(string name, int value) : this(name, CLValue.I32(value))
        {
        }

        public NamedArg(string name, long value) : this(name, CLValue.I64(value))
        {
        }

        public NamedArg(string name, byte value) : this(name, CLValue.U8(value))
        {
        }
        
        public NamedArg(string name, uint value) : this(name, CLValue.U32(value))
        {
        }
        
        public NamedArg(string name, ulong value) : this(name, CLValue.U64(value))
        {
        }
        
        public NamedArg(string name, string value) : this(name, CLValue.String(value))
        {
        }

        public NamedArg(string name, URef value) : this(name, CLValue.URef(value))
        {
        }

        public NamedArg(string name, byte[] value) : this(name, CLValue.ByteArray(value))
        {
        }
        
        public NamedArg(string name, PublicKey value) : this(name, CLValue.PublicKey(value))
        {
        }
        
        public class NamedArgConverter: JsonConverter<NamedArg>
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
                if(reader.TokenType == JsonTokenType.StartObject)
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
    }
}
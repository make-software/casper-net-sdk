using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Converters
{
    public class CLTypeInfoConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(CLTypeInfo);
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(CLTypeInfoConverterInner),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);
        }

        public class CLTypeInfoConverterInner : JsonConverter<CLTypeInfo>
        {
            public CLTypeInfoConverterInner(JsonSerializerOptions options)
            {
            }

            public override CLTypeInfo Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                return ReadCLType(ref reader);
            }

            public CLTypeInfo ReadCLType(ref Utf8JsonReader reader)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    
                    if(!CLType.TryParse(reader.GetString(), out CLType clType))
                        throw new JsonException($"Unable to convert \"{value}\" to CLType Enum.");
                    
                    return new CLTypeInfo(clType);
                }
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read(); //StartObject
                    var propertyName = reader.GetString();
                    reader.Read();
                    if (propertyName == "Option")
                    {
                        var innerType = ReadCLType(ref reader);
                        reader.Read(); //End object

                        return new CLOptionTypeInfo(innerType);
                    }
                    else if (propertyName == "List")
                    {
                        var innerType = ReadCLType(ref reader);
                        reader.Read(); //End object

                        return new CLListTypeInfo(innerType);
                    }
                    else if (propertyName == "ByteArray")
                    {
                        var size = reader.GetInt32();
                        reader.Read(); //End object
                        return new CLByteArrayTypeInfo(size);
                    }
                    else if (propertyName == "Result")
                    {
                        reader.Read(); // start object
                        reader.GetString(); // 'ok' TODO: do not assume that ok comes always first
                        reader.Read();
                        var okTypeInfo = ReadCLType(ref reader);
                        reader.Read();
                        reader.GetString(); // 'err'
                        reader.Read();
                        var errTypeInfo = ReadCLType(ref reader);
                        reader.Read();
                        reader.Read(); // end object

                        return new CLResultTypeInfo(okTypeInfo, errTypeInfo);
                    }
                    else if (propertyName == "Map")
                    {
                        reader.Read(); // start object
                        reader.GetString(); // 'key' TODO: do not assume that ok comes always first
                        reader.Read();
                        var okTypeInfo = ReadCLType(ref reader);
                        reader.Read();
                        reader.GetString(); // 'type'
                        reader.Read();
                        var errTypeInfo = ReadCLType(ref reader);
                        reader.Read();
                        reader.Read(); // end object

                        return new CLMapTypeInfo(okTypeInfo, errTypeInfo);
                    }
                    else if (propertyName == "Tuple1")
                    {
                        reader.Read(); // start array
                        var t0 = ReadCLType(ref reader);
                        reader.Read(); 
                        reader.Read(); // end array

                        return new CLTuple1TypeInfo(t0);
                    }
                    else if (propertyName == "Tuple2")
                    {
                        reader.Read(); // start array
                        var t0 = ReadCLType(ref reader);
                        reader.Read();
                        var t1 = ReadCLType(ref reader);
                        reader.Read();
                        reader.Read(); // end array
                        
                        return new CLTuple2TypeInfo(t0, t1);
                    }
                    else if (propertyName == "Tuple3")
                    {
                        reader.Read(); // start array
                        var t0 = ReadCLType(ref reader);
                        reader.Read();
                        var t1 = ReadCLType(ref reader);
                        reader.Read();
                        var t2 = ReadCLType(ref reader);
                        reader.Read();
                        reader.Read(); // end array
                        
                        return new CLTuple3TypeInfo(t0, t1, t2);
                    }
                    throw new JsonException($"CLType \"{propertyName}\" unknown or not supported");
                }
                throw new JsonException($"CLType unknown or not supported");
            }
            
            public override void Write(
                Utf8JsonWriter writer,
                CLTypeInfo clTypeInfo,
                JsonSerializerOptions options)
            {
                // write type and inner types (if any) recursively
                //
                WriteCLType(writer, clTypeInfo);
            }

            private void WriteCLType(Utf8JsonWriter writer, CLTypeInfo typeInfo)
            {
                if (typeInfo is CLKeyTypeInfo clKey)
                {
                    writer.WriteStringValue("Key");
                }
                if (typeInfo is CLOptionTypeInfo clOption)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Option");
                    WriteCLType(writer, clOption.OptionType);
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLByteArrayTypeInfo clByteArray)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("ByteArray", clByteArray.Size);
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLResultTypeInfo clResult)
                { 
                    writer.WriteStartObject();
                    writer.WritePropertyName("Result");
                    writer.WriteStartObject();
                    writer.WritePropertyName("ok");
                    WriteCLType(writer, clResult.Ok);
                    writer.WritePropertyName("err");
                    WriteCLType(writer, clResult.Err);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLMapTypeInfo clMap)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Map");
                    writer.WriteStartObject();
                    writer.WritePropertyName("key");
                    WriteCLType(writer, clMap.KeyType);
                    writer.WritePropertyName("value");
                    WriteCLType(writer, clMap.ValueType);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLListTypeInfo clList)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("List");
                    WriteCLType(writer, clList.ListType);
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLTuple1TypeInfo clTuple1)
                {
                    writer.WriteStartObject();
                    writer.WriteStartArray("Tuple1");
                    WriteCLType(writer, clTuple1.Type0);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLTuple2TypeInfo clTuple2)
                {
                    writer.WriteStartObject();
                    writer.WriteStartArray("Tuple2");
                    WriteCLType(writer, clTuple2.Type0);
                    WriteCLType(writer, clTuple2.Type1);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else if (typeInfo is CLTuple3TypeInfo clTuple3)
                {
                    writer.WriteStartObject();
                    writer.WriteStartArray("Tuple3");
                    WriteCLType(writer, clTuple3.Type0);
                    WriteCLType(writer, clTuple3.Type1);
                    WriteCLType(writer, clTuple3.Type2);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStringValue(typeInfo.ToString());
                }
            }
        }
    }
}
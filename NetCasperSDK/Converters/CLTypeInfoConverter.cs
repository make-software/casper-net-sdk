using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.Converters
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
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    
                    if(!CLType.TryParse(reader.GetString(), out CLType clType))
                        throw new JsonException($"Unable to convert \"{value}\" to CLType Enum.");
                    
                    return new CLTypeInfo(clType);
                }
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read();
                    var propertyName = reader.GetString();
                    reader.Read();
                    if (propertyName == "Option")
                    {
                        var value = reader.GetString();
                        if(!CLType.TryParse(reader.GetString(), out CLType clType))
                            throw new JsonException($"Unable to convert \"{value}\" to CLType Enum.");
                        reader.Read(); //End object
                        return new CLOptionTypeInfo(new CLTypeInfo(clType));
                    }
                    else if (propertyName == "List")
                    {
                        var value = reader.GetString();
                        if(!CLType.TryParse(reader.GetString(), out CLType clType))
                            throw new JsonException($"Unable to convert \"{value}\" to CLType Enum.");
                        reader.Read(); //End object
                        return new CLListTypeInfo(new CLTypeInfo(clType));
                    }
                    else if (propertyName == "ByteArray")
                    {
                        var size = reader.GetInt32();
                        reader.Read(); //End object
                        return new CLByteArrayTypeInfo(size);
                    }
                    else if (propertyName == "Tuple1")
                    {
                        var value = reader.GetString();
                        if(!CLType.TryParse(reader.GetString(), out CLType clType))
                            throw new JsonException($"Unable to convert \"{value}\" to CLType Enum.");
                        reader.Read(); //End object
                        return new CLTuple1TypeInfo(new CLTypeInfo(clType));
                    }
                    else if (propertyName == "Tuple2")
                    {
                        reader.Read(); // start array
                        var t0 = reader.GetString();
                        reader.Read();
                        var t1 = reader.GetString();
                        reader.Read();
                        reader.Read(); // end array
                        
                        if(!CLType.TryParse(t0, out CLType clType0))
                            throw new JsonException($"Unable to convert \"{t0}\" to CLType Enum.");
                        if(!CLType.TryParse(t1, out CLType clType1))
                            throw new JsonException($"Unable to convert \"{t1}\" to CLType Enum.");
                        //reader.Read(); //End object
                        return new CLTuple2TypeInfo(new CLTypeInfo(clType0), new CLTypeInfo(clType1));
                    }
                    else if (propertyName == "Tuple3")
                    {
                        reader.Read(); // start array
                        var t0 = reader.GetString();
                        reader.Read();
                        var t1 = reader.GetString();
                        reader.Read();
                        var t2 = reader.GetString();
                        reader.Read();
                        reader.Read(); // end array
                        
                        if(!CLType.TryParse(t0, out CLType clType0))
                            throw new JsonException($"Unable to convert \"{t0}\" to CLType Enum.");
                        if(!CLType.TryParse(t1, out CLType clType1))
                            throw new JsonException($"Unable to convert \"{t1}\" to CLType Enum.");
                        if(!CLType.TryParse(t2, out CLType clType2))
                            throw new JsonException($"Unable to convert \"{t2}\" to CLType Enum.");
                        //reader.Read(); //End object
                        return new CLTuple3TypeInfo(new CLTypeInfo(clType0), new CLTypeInfo(clType1), new CLTypeInfo(clType2));
                    }
                    throw new JsonException($"CLType \"{propertyName}\" not yet supported");
                }
                throw new JsonException($"CLType not yet supported");
            }
            
            public override void Write(
                Utf8JsonWriter writer,
                CLTypeInfo clTypeInfo,
                JsonSerializerOptions options)
            {
                if (clTypeInfo is CLOptionTypeInfo clOption)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Option", clOption.OptionType.Type.ToString());
                    writer.WriteEndObject();
                }
                else if (clTypeInfo is CLListTypeInfo clList)
                {
                    writer.WriteStartObject();
                    writer.WriteString("List", clList.ListType.Type.ToString());
                    writer.WriteEndObject();
                }
                else if (clTypeInfo is CLByteArrayTypeInfo clByteArray)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("ByteArray", clByteArray.Size);
                    writer.WriteEndObject();
                }
                else if (clTypeInfo is CLTuple1TypeInfo clTuple1)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Tuple1", clTuple1.Type0.ToString());
                    writer.WriteEndObject();
                }
                else if (clTypeInfo is CLTuple2TypeInfo clTuple2)
                {
                    writer.WriteStartObject();
                    writer.WriteStartArray("Tuple2");
                    writer.WriteStringValue(clTuple2.Type0.ToString());
                    writer.WriteStringValue(clTuple2.Type1.ToString());
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else if (clTypeInfo is CLTuple3TypeInfo clTuple3)
                {
                    writer.WriteStartObject();
                    writer.WriteStartArray("Tuple3");
                    writer.WriteStringValue(clTuple3.Type0.ToString());
                    writer.WriteStringValue(clTuple3.Type1.ToString());
                    writer.WriteStringValue(clTuple3.Type2.ToString());
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStringValue(clTypeInfo.ToString());
                }
            }
        }
    }
}
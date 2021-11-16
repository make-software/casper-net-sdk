using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Access control options for a contract entry point (method).
    /// </summary>
    public class EntryPointAccess
    {
        public bool IsPublic { get; init; }

        /// <summary>
        /// Only users from the listed groups may call this method.
        /// Note: if the list is empty then this method is not callable from outside the contract.
        /// </summary>
        public List<string> Groups { get; init; }

        public class EntryPointAccessConverter : JsonConverter<EntryPointAccess>
        {
            public override EntryPointAccess Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    if (value.ToLower() == "public")
                        return new EntryPointAccess()
                        {
                            IsPublic = true,
                            Groups = null
                        };
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read(); // start object
                    var value = reader.GetString(); // read property
                    reader.Read();
                    if (value.ToLower() == "groups")
                    {
                        List<string> groups = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                        reader.Read(); // end array

                        return new EntryPointAccess()
                        {
                            IsPublic = false,
                            Groups = groups
                        };
                    }
                }

                throw new JsonException(
                    $"Could not deserialize EntryPointAccess. Not expected token found.");
            }

            public override void Write(
                Utf8JsonWriter writer,
                EntryPointAccess value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for EntryPointAccess not yet implemented");
            }
        }
    }

    public enum EntryPointType
    {
        Session,
        Contract
    }

    /// <summary>
    /// Parameter to a method
    /// </summary>
    public class Parameter
    {
        [JsonPropertyName("name")] 
        public string Name { get; init; }

        [JsonPropertyName("cl_type")]
        [JsonConverter(typeof(CLTypeInfoConverter))]
        public CLTypeInfo CLType { get; init; }
    }
    
    /// <summary>
    /// Type signature of a method. Order of arguments matter since can be referenced by index as well as name.
    /// </summary>
    public class EntryPoint
    {
        /// <summary>
        /// Access control options for a contract entry point
        /// </summary>
        [JsonPropertyName("access")]
        [JsonConverter(typeof(EntryPointAccess.EntryPointAccessConverter))]
        public EntryPointAccess Access { get; init; }

        /// <summary>
        /// List of input parameters to the method. Order of arguments matter since can be referenced by index as well as name.
        /// </summary>
        [JsonPropertyName("args")]
        public List<Parameter> Args { get; init; }

        /// <summary>
        /// Context of method execution
        /// </summary>
        [JsonPropertyName("entry_point_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EntryPointType EntryPointType { get; init; }

        /// <summary>
        /// Name of the entry point
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; }

        /// <summary>
        /// Returned value
        /// </summary>
        [JsonPropertyName("ret")]
        [JsonConverter(typeof(CLTypeInfoConverter))]
        public CLTypeInfo Ret { get; init; }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Access control options for a contract entry point (method).
    /// </summary>
    public class EntryPointAccess
    {
        /// <summary>
        /// When public, anyone can call this method (no access controls).
        /// </summary>
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
                    if (value.ToLowerInvariant() == "public")
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
                    if (value.ToLowerInvariant() == "groups")
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

    /// <summary>
    /// Defines whether the code runs in the contract's or the session context.
    /// </summary>
    public enum EntryPointType
    {
        /// <summary>
        /// Runs as session code
        /// </summary>
        Session,
        /// <summary>
        /// Runs within contract’s context
        /// </summary>
        Contract
    }

    /// <summary>
    /// Parameter to a method
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// The name of the parameter in the entry point.
        /// </summary>
        [JsonPropertyName("name")] 
        public string Name { get; init; }

        /// <summary>
        /// The type of the parameter in the entry point.
        /// </summary>
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
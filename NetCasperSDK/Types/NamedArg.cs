using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    [JsonConverter(typeof(NamedArgConverter))]
    public class NamedArg
    {
        public string Name { get; }
        public CLValue Value { get; }

        public NamedArg(string name, CLValue value)
        {
            Name = name;
            Value = value;
        }
    }
}
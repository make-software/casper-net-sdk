using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.Converters
{
    public class PublicKeyConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(PublicKey);
        }
        
        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(PublicKeyInner),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;            
        }

        public class PublicKeyInner : JsonConverter<PublicKey>
        {
            public PublicKeyInner(JsonSerializerOptions options)
            {
            }

            public override PublicKey Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var hex = reader.GetString();
                reader.Read();
                
                return PublicKey.FromHexString(hex);
            }

            public override void Write(
                Utf8JsonWriter writer,
                PublicKey publicKey,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(publicKey.ToAccountHex());
            }
        }
    }
}
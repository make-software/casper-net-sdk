using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A validator's public key paired with its weight, i.e. the total number of motes staked by it and its delegators.
    /// </summary>
    public class ValidatorWeight
    {
        /// <summary>
        /// Validator's Public Key.
        /// </summary>
        [JsonConverter(typeof(PublicKey.PublicKeyConverter))]
        public PublicKey PublicKey { get; init; }

        /// <summary>
        /// Validator's weight.
        /// </summary>
        public BigInteger Weight { get; init; }

        public class ValidatorWeightConverter : JsonConverter<ValidatorWeight>
        {
            public override ValidatorWeight Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("StartObject token expected to deserialize a ValidatorWeight");

                reader.Read(); // Start object

                string publicKey = string.Empty;
                BigInteger weight = BigInteger.Zero;

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();
                    switch (property)
                    {
                        case "public_key":
                        case "validator":
                            publicKey = reader.GetString();
                            reader.Read();
                            break;
                        case "weight":
                            weight = BigInteger.Parse(reader.GetString() ?? "0");
                            reader.Read();
                            break;
                    }
                }

                return new ValidatorWeight()
                {
                    PublicKey = PublicKey.FromHexString(publicKey),
                    Weight = weight
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                ValidatorWeight value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Serialization of ValidatorWeight not yet implemented");
            }
        }
    }
}
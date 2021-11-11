using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCasperSDK.Types
{
    public enum TransformType
    {
        Identity,
        WriteContractWasm,
        WriteContract,
        WriteContractPackage,
        WriteCLValue,
        WriteAccount,
        WriteDeployInfo,
        WriteEraInfo,
        WriteTransfer,
        WriteBid,
        WriteWithdraw,
        AddInt32,
        AddUInt64,
        AddUInt128,
        AddUInt256,
        AddUInt512,
        AddKeys,
        Failure
    }

    /// <summary>
    /// A transformation performed while executing a deploy.
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// The formatted string of the `Key`.
        /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// The type of transform
        /// </summary>
        public TransformType Type { get; init; }

        /// <summary>
        /// Data associated to some type of transforms
        /// </summary>
        public object Value { get; init; }

        public class TransformConverter : JsonConverter<Transform>
        {
            public override Transform Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize Transform. StartObject expected");

                reader.Read(); // start object

                string key = null;
                TransformType? type = null;
                object value = null;

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var field = reader.GetString();
                    reader.Read();
                    if (field == "key")
                    {
                        key = reader.GetString();
                        reader.Read();
                    }
                    else if (field == "transform")
                    {
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            var stype = reader.GetString();
                            if (stype != null)
                                type = Enum.Parse<TransformType>(stype);
                            reader.Read();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            reader.Read();
                            var stype = reader.GetString();
                            if (stype != null)
                                type = Enum.Parse<TransformType>(stype);
                            reader.Read();
                            switch (type)
                            {
                                case TransformType.WriteCLValue:
                                    value = JsonSerializer.Deserialize<CLValue>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteAccount:
                                    value = JsonSerializer.Deserialize<Account>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteDeployInfo:
                                    value = JsonSerializer.Deserialize<DeployInfo>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteEraInfo:
                                    value = JsonSerializer.Deserialize<EraInfo>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteTransfer:
                                    value = JsonSerializer.Deserialize<Transfer>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteBid:
                                    value = JsonSerializer.Deserialize<Bid>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.WriteWithdraw:
                                    value = JsonSerializer.Deserialize<UnbondingPurse>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformType.AddInt32:
                                    value = reader.GetInt32();
                                    reader.Read();
                                    break;
                                case TransformType.AddUInt64:
                                    value = reader.GetUInt64();
                                    reader.Read();
                                    break;
                                case TransformType.AddUInt128:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformType.AddUInt256:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformType.AddUInt512:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformType.AddKeys:
                                    value = JsonSerializer.Deserialize<List<NamedKey>>(ref reader, options);
                                    reader.Read(); // end array
                                    break;
                                case TransformType.Failure:
                                    value = reader.GetString();
                                    reader.Read();
                                    break;
                            }

                            reader.Read(); //end object
                        }
                    }
                }

                if (key != null & type != null)
                {
                    return new Transform()
                    {
                        Key = key,
                        Type = type ?? TransformType.Identity,
                        Value = value
                    };
                }

                throw new JsonException("Incomplete transform");
            }

            public override void Write(
                Utf8JsonWriter writer,
                Transform value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for Transform not yet implemented");
            }
        }
    }
}
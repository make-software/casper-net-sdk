using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Enumeration of transformation types used in the execution of a deploy.
    /// </summary>
    public enum TransformKindV1
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
        Failure,
        WriteUnbonding,
        WriteAddressableEntity,
        Prune,
        WriteBidKind,
    }

    /// <summary>
    /// A transformation performed while executing a deploy.
    /// </summary>
    public class TransformV1
    {
        /// <summary>
        /// The formatted string of the `Key`.
        /// </summary>
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public GlobalStateKey Key { get; init; }

        /// <summary>
        /// The type of transform
        /// </summary>
        public TransformKindV1 Kind { get; init; }

        /// <summary>
        /// Data associated to some type of transforms
        /// </summary>
        public object Value { get; init; }

        public class TransformV1Converter : JsonConverter<TransformV1>
        {
            public override TransformV1 Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize Transform. StartObject expected");

                reader.Read(); // start object

                string key = null;
                TransformKindV1? type = null;
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
                                type = EnumCompat.Parse<TransformKindV1>(stype);
                            reader.Read();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            reader.Read();
                            var stype = reader.GetString();
                            if (stype != null)
                                type = EnumCompat.Parse<TransformKindV1>(stype);
                            reader.Read();
                            switch (type)
                            {
                                case TransformKindV1.WriteCLValue:
                                    value = JsonSerializer.Deserialize<CLValue>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteAccount:
                                    value = GlobalStateKey.FromString(reader.GetString());
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteDeployInfo:
                                    value = JsonSerializer.Deserialize<DeployInfo>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteEraInfo:
                                    value = JsonSerializer.Deserialize<EraInfo>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteTransfer:
                                    value = JsonSerializer.Deserialize<TransferV1>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteBid:
                                    value = JsonSerializer.Deserialize<Bid>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.WriteWithdraw:
                                    value = JsonSerializer.Deserialize<List<WithdrawPurse>>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV1.AddInt32:
                                    value = reader.GetInt32();
                                    reader.Read();
                                    break;
                                case TransformKindV1.AddUInt64:
                                    value = reader.GetUInt64();
                                    reader.Read();
                                    break;
                                case TransformKindV1.AddUInt128:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV1.AddUInt256:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV1.AddUInt512:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV1.AddKeys:
                                    value = JsonSerializer.Deserialize<List<NamedKey>>(ref reader, options);
                                    reader.Read(); // end array
                                    break;
                                case TransformKindV1.Failure:
                                    value = reader.GetString();
                                    reader.Read();
                                    break;
                                case TransformKindV1.WriteUnbonding:
                                    value = JsonSerializer.Deserialize<List<UnbondingPurse>>(ref reader, options);
                                    reader.Read();
                                    break;
                                case TransformKindV1.Prune:
                                    value = GlobalStateKey.FromString(reader.GetString());
                                    reader.Read();
                                    break;  
                                case TransformKindV1.WriteBidKind:
                                    value = JsonSerializer.Deserialize<BidKind>(ref reader, options);
                                    reader.Read();
                                    break;  
                            }

                            reader.Read(); //end object
                        }
                    }
                }

                if (key != null & type != null)
                {
                    return new TransformV1()
                    {
                        Key = GlobalStateKey.FromString(key),
                        Kind = type ?? TransformKindV1.Identity,
                        Value = value
                    };
                }

                throw new JsonException("Incomplete transform");
            }

            public override void Write(
                Utf8JsonWriter writer,
                TransformV1 value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for Transform not yet implemented");
            }
        }
    }
}

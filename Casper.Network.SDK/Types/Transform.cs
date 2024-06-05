using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public enum TransformKind
    {
        /// <summary>
        /// An identity transformation that does not modify a value in the global state. Created as a result of
        /// reading from the global state.
        /// </summary>
        Identity,

        /// <summary>
        /// Writes a new value (StoredValue) in the global state.
        /// </summary>
        Write,

        /// <summary>
        /// A wrapping addition of an `i32` to an existing numeric value (not necessarily an `i32`) in the global state.
        /// </summary>
        AddInt32,

        /// <summary>
        /// A wrapping addition of a `u64` to an existing numeric value (not necessarily an `u64`) in the global state.
        /// </summary>
        AddUInt64,

        /// <summary>
        /// A wrapping addition of a `U128` to an existing numeric value (not necessarily an `U128`) in the global state.
        /// </summary>
        AddUInt128,

        /// <summary>
        /// A wrapping addition of a `U256` to an existing numeric value (not necessarily an `U256`) in the global state.
        /// </summary>
        AddUInt256,

        /// <summary>
        /// A wrapping addition of a `U512` to an existing numeric value (not necessarily an `U512`) in the global state.
        /// </summary>
        AddUInt512,

        /// <summary>
        /// Adds new named keys to an existing entry in the global state.\n\nThis transform assumes that the existing stored
        /// value is either an Account or a Contract.
        /// </summary>
        AddKeys,

        /// <summary>
        /// Removes the pathing to the global state entry of the specified key. The pruned element remains reachable from
        /// previously generated global state root hashes, but will not be included in the next generated global state
        /// root hash and subsequent state accumulated from it.
        /// </summary>
        Prune,

        /// <summary>
        /// Represents the case where applying a transform would cause an error.
        /// </summary>
        Failure,
    }

    /// <summary>
    /// Representation of a single transformation occurring during execution.\n\nNote that all arithmetic
    /// variants of `TransformKindV2` are commutative which means that a given collection of them can be
    /// executed in any order to produce the same end result.
    /// </summary>
    public class Kind
    {
    }

    /// <summary>
    /// A transformation performed while executing a deploy.
    /// </summary>
    public class Transform
    {
        protected int _version;

        /// <summary>
        /// Returns the version of the block.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        
        protected TransformV1 _transformV1;

        public static explicit operator TransformV1(Transform transform)
        {
            if(transform._version == 1)
                return transform._transformV1;

            throw new InvalidCastException("Version2 transform cannot be converted to Version1");
        }
        
        public static explicit operator Transform(TransformV1 transform)
        {
            TransformKind kind = transform.Kind switch
            {
                TransformKindV1.Identity => TransformKind.Identity,
                TransformKindV1.WriteAccount => TransformKind.Write,
                TransformKindV1.WriteAddressableEntity => TransformKind.Write,
                TransformKindV1.WriteBid => TransformKind.Write,
                TransformKindV1.WriteBidKind => TransformKind.Write,
                TransformKindV1.WriteCLValue => TransformKind.Write,
                TransformKindV1.WriteContract => TransformKind.Write,
                TransformKindV1.WriteContractPackage => TransformKind.Write,
                TransformKindV1.WriteContractWasm => TransformKind.Write,
                TransformKindV1.WriteDeployInfo => TransformKind.Write,
                TransformKindV1.WriteEraInfo => TransformKind.Write,
                TransformKindV1.WriteTransfer => TransformKind.Write,
                TransformKindV1.WriteUnbonding => TransformKind.Write,
                TransformKindV1.WriteWithdraw => TransformKind.Write,
                TransformKindV1.AddInt32 => TransformKind.AddInt32,
                TransformKindV1.AddUInt64 => TransformKind.AddUInt64,
                TransformKindV1.AddUInt128 => TransformKind.AddUInt128,
                TransformKindV1.AddUInt256 => TransformKind.AddUInt256,
                TransformKindV1.AddUInt512 => TransformKind.AddUInt512,
                TransformKindV1.AddKeys => TransformKind.AddKeys,
                TransformKindV1.Failure => TransformKind.Failure,
                TransformKindV1.Prune => TransformKind.Prune,
            };

            return new Transform
            {
                Key = transform.Key,
                TransformKind = kind,
                Value = transform.Value,
            };
        }
        
        /// <summary>
        /// The formatted string of the `Key`.
        /// </summary>
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public GlobalStateKey Key { get; init; }

        /// <summary>
        /// Representation of a single transformation occurring during execution.\n\nNote that all arithmetic
        /// variants of `TransformKind` are commutative which means that a given collection of them can be
        /// executed in any order to produce the same end result.
        /// </summary>
        public TransformKind TransformKind { get; init; }

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
                TransformKind? kind = null;
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
                    else if (field == "kind")
                    {
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            var stype = reader.GetString();
                            if (stype != null)
                                kind = EnumCompat.Parse<TransformKind>(stype);
                            reader.Read();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            reader.Read();
                            var stype = reader.GetString();
                            if (stype != null)
                                kind = EnumCompat.Parse<TransformKind>(stype);
                            reader.Read();
                            switch (kind)
                            {
                                case TransformKind.Write:
                                    value = JsonSerializer.Deserialize<StoredValue>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKind.AddInt32:
                                    value = reader.GetInt32();
                                    reader.Read();
                                    break;
                                case TransformKind.AddUInt64:
                                    value = reader.GetUInt64();
                                    reader.Read();
                                    break;
                                case TransformKind.AddUInt128:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKind.AddUInt256:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKind.AddUInt512:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKind.AddKeys:
                                    value = JsonSerializer.Deserialize<List<NamedKeyValue>>(ref reader, options);
                                    reader.Read(); // end array
                                    break;
                                case TransformKind.Prune:
                                    value = GlobalStateKey.FromString(reader.GetString());
                                    reader.Read();
                                    break;
                                case TransformKind.Failure:
                                    value = reader.GetString();
                                    reader.Read();
                                    break;
                            }

                            reader.Read(); //end object
                        }
                    }
                }

                if (key != null & kind != null)
                {
                    return new Transform()
                    {
                        Key = GlobalStateKey.FromString(key),
                        TransformKind = kind ?? TransformKind.Identity,
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
                throw new NotImplementedException("Write method for TransformV2 not yet implemented");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public enum TransformKindV2
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
    public class TransformV2
    {
        /// <summary>
        /// The formatted string of the `Key`.
        /// </summary>
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public GlobalStateKey Key { get; init; }
        
        /// <summary>
        /// Representation of a single transformation occurring during execution.\n\nNote that all arithmetic
        /// variants of `TransformKindV2` are commutative which means that a given collection of them can be
        /// executed in any order to produce the same end result.
        /// </summary>
        public TransformKindV2 TransformKind { get; init; }
        
        /// <summary>
        /// Data associated to some type of transforms
        /// </summary>
        public object Value { get; init; }
        
        public class TransformV2Converter : JsonConverter<TransformV2>
        {
            public override TransformV2 Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize Transform. StartObject expected");

                reader.Read(); // start object

                string key = null;
                TransformKindV2? kind = null;
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
                                kind = EnumCompat.Parse<TransformKindV2>(stype);
                            reader.Read();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            reader.Read();
                            var stype = reader.GetString();
                            if (stype != null)
                                kind = EnumCompat.Parse<TransformKindV2>(stype);
                            reader.Read();
                            switch (kind)
                            {
                                case TransformKindV2.Write:
                                    value = JsonSerializer.Deserialize<StoredValue>(ref reader, options);
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV2.AddInt32:
                                    value = reader.GetInt32();
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt64:
                                    value = reader.GetUInt64();
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt128:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt256:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt512:
                                    value = BigInteger.Parse(reader.GetString() ?? "0");
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddKeys:
                                    value = JsonSerializer.Deserialize<List<NamedKeyValue>>(ref reader, options);
                                    reader.Read(); // end array
                                    break;
                                case TransformKindV2.Prune:
                                    value = GlobalStateKey.FromString(reader.GetString());
                                    reader.Read();
                                    break;
                                case TransformKindV2.Failure:
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
                    return new TransformV2()
                    {
                        Key = GlobalStateKey.FromString(key),
                        TransformKind = kind ?? TransformKindV2.Identity,
                        Value = value
                    };
                }

                throw new JsonException("Incomplete transform");
            }

            public override void Write(
                Utf8JsonWriter writer,
                TransformV2 value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for TransformV2 not yet implemented");
            }
        }
    }
}
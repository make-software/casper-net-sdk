using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public enum TransformKindV2
    {
        Identity,
        Write,
        AddInt32,
        AddUInt64,
        AddUInt128,
        AddUInt256,
        AddUInt512,
        AddKeys,
        Prune,
        Failure,
    }

    public abstract class TransformKind
    {
        public object Value { get; init; }
    }

    public abstract class TransformKind<T> : TransformKind
    {
        public new abstract T Value { get; init; }
    }

    /// <summary>
    /// An identity transformation that does not modify a value in the global state. Created as a result of
    /// reading from the global state.
    /// </summary>
    public class IdentityTransformKind : TransformKind<object>
    {
        public override object Value { get; init; }
    }

    /// <summary>
    /// Writes a new value (StoredValue) in the global state.
    /// </summary>
    public class WriteTransformKind : TransformKind<StoredValue>
    {
        public override StoredValue Value { get; init; }
    }

    /// <summary>
    /// A wrapping addition of an `i32` to an existing numeric value (not necessarily an `i32`) in the global state.
    /// </summary>
    public class AddInt32TransformKind : TransformKind<int>
    {
        public override int Value { get; init; }
    }

    /// <summary>
    /// A wrapping addition of a `u64` to an existing numeric value (not necessarily an `u64`) in the global state.
    /// </summary>
    public class AddUInt64TransformKind : TransformKind<ulong>
    {
        public override ulong Value { get; init; }
    }

    /// <summary>
    /// A wrapping addition of a `U128` to an existing numeric value (not necessarily an `U128`) in the global state.
    /// </summary>
    public class AddUInt128TransformKind : TransformKind<BigInteger>
    {
        public override BigInteger Value { get; init; }
    }

    /// <summary>
    /// A wrapping addition of a `U256` to an existing numeric value (not necessarily an `U256`) in the global state.
    /// </summary>
    public class AddUInt256TransformKind : TransformKind<BigInteger>
    {
        public override BigInteger Value { get; init; }
    }

    /// <summary>
    /// A wrapping addition of a `U512` to an existing numeric value (not necessarily an `U512`) in the global state.
    /// </summary>
    public class AddUInt512TransformKind : TransformKind<BigInteger>
    {
        public override BigInteger Value { get; init; }
    }

    /// <summary>
    /// Adds new named keys to an existing entry in the global state.\n\nThis transform assumes that the existing stored
    /// value is either an Account or a Contract.
    /// </summary>
    public class AddKeysTransformKind : TransformKind<List<NamedKey>>
    {
        public override List<NamedKey> Value { get; init; }
    }

    /// <summary>
    /// Removes the pathing to the global state entry of the specified key. The pruned element remains reachable from
    /// previously generated global state root hashes, but will not be included in the next generated global state
    /// root hash and subsequent state accumulated from it.
    /// </summary>
    public class PruneTransformKind : TransformKind<GlobalStateKey>
    {
        public override GlobalStateKey Value { get; init; }
    }

    /// <summary>
    /// Represents the case where applying a transform would cause an error.
    /// </summary>
    public class FailureTransformKind : TransformKind<string>
    {
        public override string Value { get; init; }
    }

    public class WriteContractLegacyTransformKind : TransformKind<object>
    {
        public override object Value { get; init; }
    }

    public class WriteContractPackageLegacyTransformKind : TransformKind<object>
    {
        public override object Value { get; init; }
    }

    public class WriteContractWasmLegacyTransformKind : TransformKind<object>
    {
        public override object Value { get; init; }
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
            if (transform._version == 1)
                return transform._transformV1;

            throw new InvalidCastException("Version2 transform cannot be converted to Version1");
        }

        public static explicit operator Transform(TransformV1 transform)
        {
            TransformKind kind = transform.Kind switch
            {
                TransformKindV1.Identity => new IdentityTransformKind(),
                TransformKindV1.WriteAccount => new WriteTransformKind()
                    { Value = new StoredValue() { Account = transform.Value as Account } },
                // TransformKindV1.WriteAddressableEntity => new WriteTransformKind()
                //     { Value = new StoredValue() { AddressableEntity = transform.Value as AddressableEntity } },
                TransformKindV1.WriteBid => new WriteTransformKind()
                    { Value = new StoredValue() { Bid = transform.Value as Bid } },
                // TransformKindV1.WriteBidKind => new WriteTransformKind()
                //     { Value = new StoredValue() { BidKind = transform.Value as BidKind } },
                TransformKindV1.WriteCLValue => new WriteTransformKind()
                    { Value = new StoredValue() { CLValue = transform.Value as CLValue } },
                TransformKindV1.WriteContract => new WriteContractLegacyTransformKind(),
                TransformKindV1.WriteContractPackage => new WriteContractPackageLegacyTransformKind(),
                TransformKindV1.WriteContractWasm => new WriteContractWasmLegacyTransformKind(),
                TransformKindV1.WriteDeployInfo => new WriteTransformKind()
                    { Value = new StoredValue() { DeployInfo = transform.Value as DeployInfo } },
                TransformKindV1.WriteEraInfo => new WriteTransformKind()
                    { Value = new StoredValue() { EraInfo = transform.Value as EraInfo } },
                TransformKindV1.WriteTransfer => new WriteTransformKind()
                    { Value = new StoredValue() { Transfer = (Transfer)(transform.Value as TransferV1) } },
                TransformKindV1.WriteUnbonding => new WriteTransformKind()
                    { Value = new StoredValue() { Unbonding = transform.Value as List<UnbondingPurse> } },
                TransformKindV1.WriteWithdraw => new WriteTransformKind()
                    { Value = new StoredValue() { Withdraw = transform.Value as List<WithdrawPurse> } },
                TransformKindV1.AddInt32 => new AddInt32TransformKind() { Value = (int)transform.Value },
                TransformKindV1.AddUInt64 => new AddUInt64TransformKind() { Value = (ulong)transform.Value },
                TransformKindV1.AddUInt128 => new AddUInt128TransformKind() { Value = (BigInteger)transform.Value },
                TransformKindV1.AddUInt256 => new AddUInt256TransformKind() { Value = (BigInteger)transform.Value },
                TransformKindV1.AddUInt512 => new AddUInt512TransformKind() { Value = (BigInteger)transform.Value },
                TransformKindV1.AddKeys => new AddKeysTransformKind() { Value = transform.Value as List<NamedKey> },
                TransformKindV1.Failure => new FailureTransformKind() { Value = (string)transform.Value },
                // TransformKindV1.Prune => new PruneTransformKind() {Value = transform.Value as GlobalStateKey },
                _ => throw new Exception("Cannot convert '" + transform.Kind + "' to TransofrmKind v2."),
            };

            return new Transform
            {
                _version = 1,
                _transformV1 = transform,
                Key = transform.Key,
                Kind = kind,
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
        public TransformKind Kind { get; init; }

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
                            if (TransformKindV2.Identity.ToString().Equals(reader.GetString()))
                                kind = new IdentityTransformKind();
                            else
                                throw new Exception("Unexpected transform kind: " + reader.GetString());
                            reader.Read();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            reader.Read(); // skip start object
                            var stype = reader.GetString();
                            var kindv2 = EnumCompat.Parse<TransformKindV2>(stype);
                            reader.Read();
                            switch (kindv2)
                            {
                                case TransformKindV2.Write:
                                    var storedValue = JsonSerializer.Deserialize<StoredValue>(ref reader, options);
                                    kind = new WriteTransformKind() { Value = storedValue };
                                    reader.Read(); // end object
                                    break;
                                case TransformKindV2.AddInt32:
                                    kind = new AddInt32TransformKind() { Value = reader.GetInt32() };
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt64:
                                    kind = new AddUInt64TransformKind() { Value = reader.GetUInt64() };
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt128:
                                    var u128 = BigInteger.Parse(reader.GetString() ?? "0");
                                    kind = new AddUInt128TransformKind() { Value = u128 };
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt256:
                                    var u256 = BigInteger.Parse(reader.GetString() ?? "0");
                                    kind = new AddUInt256TransformKind() { Value = u256 };
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddUInt512:
                                    var u512 = BigInteger.Parse(reader.GetString() ?? "0");
                                    kind = new AddUInt512TransformKind() { Value = u512 };
                                    reader.Read();
                                    break;
                                case TransformKindV2.AddKeys:
                                    var namedKeys =
                                        JsonSerializer.Deserialize<List<NamedKey>>(ref reader, options);
                                    kind = new AddKeysTransformKind() { Value = namedKeys };
                                    reader.Read(); // end array
                                    break;
                                case TransformKindV2.Prune:
                                    var prunedKey = GlobalStateKey.FromString(reader.GetString());
                                    kind = new PruneTransformKind() { Value = prunedKey };
                                    reader.Read();
                                    break;
                                case TransformKindV2.Failure:
                                    var json = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                                    kind = new FailureTransformKind() { Value = json.GetRawText() };
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
                        Kind = kind,
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
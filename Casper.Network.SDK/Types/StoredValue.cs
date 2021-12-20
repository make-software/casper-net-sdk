using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A wrapper class for different types of values stored in the global state.
    /// </summary>
    public class StoredValue
    {
        public Contract Contract { get; init; }

        public CLValue CLValue { get; init; }

        public Account Account { get; init; }

        public string ContractWasm { get; init; }

        public ContractPackage ContractPackage { get; init; }

        public Transfer Transfer { get; init; }

        public DeployInfo DeployInfo { get; init; }

        public EraInfo EraInfo { get; init; }

        public Bid Bid { get; init; }

        public List<UnbondingPurse> Withdraw { get; init; }

        public class StoredValueConverter : JsonConverter<StoredValue>
        {
            public override StoredValue Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize StoredValue. StartObject expected");

                reader.Read(); // start object

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Cannot deserialize StoredValue. PropertyName expected");

                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName.ToLower() == "contract")
                {
                    var contract = JsonSerializer.Deserialize<Contract>(ref reader, options);
                    reader.Read(); // end Contract object
                    return new StoredValue()
                    {
                        Contract = contract
                    };
                }
                else if (propertyName.ToLower() == "clvalue")
                {
                    var clValue = JsonSerializer.Deserialize<CLValue>(ref reader, options);
                    reader.Read(); // end CLValue object
                    return new StoredValue()
                    {
                        CLValue = clValue
                    };
                }
                else if (propertyName.ToLower() == "account")
                {
                    var account = JsonSerializer.Deserialize<Account>(ref reader, options);
                    reader.Read(); // end Account object
                    return new StoredValue()
                    {
                        Account = account
                    };
                }
                else if (propertyName.ToLower() == "contractwasm")
                {
                    var wasmBytes = reader.GetString();
                    reader.Read(); // wasm bytes
                    return new StoredValue()
                    {
                        ContractWasm = wasmBytes
                    };
                }
                else if (propertyName.ToLower() == "contractpackage")
                {
                    var contractPackage = JsonSerializer.Deserialize<ContractPackage>(ref reader, options);
                    reader.Read(); // end ContractPackage object
                    return new StoredValue()
                    {
                        ContractPackage = contractPackage
                    };
                }
                else if (propertyName.ToLower() == "transfer")
                {
                    var transfer = JsonSerializer.Deserialize<Transfer>(ref reader, options);
                    reader.Read(); // end Transfer object
                    return new StoredValue()
                    {
                        Transfer = transfer
                    };
                }
                else if (propertyName.ToLower() == "deployinfo")
                {
                    var deployInfo = JsonSerializer.Deserialize<DeployInfo>(ref reader, options);
                    reader.Read(); // end DeployInfo object
                    return new StoredValue()
                    {
                        DeployInfo = deployInfo
                    };
                }
                else if (propertyName.ToLower() == "erainfo")
                {
                    var eraInfo = JsonSerializer.Deserialize<EraInfo>(ref reader, options);
                    reader.Read(); // end EraInfo object
                    return new StoredValue()
                    {
                        EraInfo = eraInfo
                    };
                }
                else if (propertyName.ToLower() == "bid")
                {
                    var bid = JsonSerializer.Deserialize<Bid>(ref reader, options);
                    reader.Read(); // end Bid object
                    return new StoredValue()
                    {
                        Bid = bid
                    };
                }
                else if (propertyName.ToLower() == "withdraw")
                {
                    var withdraw = JsonSerializer.Deserialize<List<UnbondingPurse>>(ref reader, options);
                    reader.Read(); // end Withdraw object
                    return new StoredValue()
                    {
                        Withdraw = withdraw
                    };
                }

                throw new JsonException("Cannot deserialize StoredValue. Inner object not yet supported");
            }

            public override void Write(
                Utf8JsonWriter writer,
                StoredValue value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException("Write method for StoredValue not yet implemented");
            }
        }
    }
}
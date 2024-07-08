using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    public enum NativeEntryPoint
    {
        /// <summary>
        /// The `transfer` native entry point, used to transfer `Motes` from a source purse to a target purse.
        /// </summary>
        Transfer = 1,
        /// <summary>
        /// The `add_bid` native entry point, used to create or top off a bid purse.
        /// </summary>
        AddBid = 2,
        /// <summary>
        /// The `withdraw_bid` native entry point, used to decrease a stake.
        /// </summary>
        WithdrawBid = 3,
        /// <summary>
        /// The `delegate` native entry point, used to add a new delegator or increase an existing delegator's stake.
        /// </summary>
        Delegate = 4,
        /// <summary>
        /// The `undelegate` native entry point, used to reduce a delegator's stake or remove the delegator if the remaining stake is 0.
        /// </summary>
        Undelegate = 5,
        /// <summary>
        /// The `redelegate` native entry point, used to reduce a delegator's stake or remove the delegator if
        /// the remaining stake is 0, and after the unbonding delay, automatically delegate to a new validator.
        /// </summary>
        Redelegate = 6,
        /// <summary>
        /// The `activate_bid` native entry point, used to used to reactivate an inactive bid.
        /// </summary>
        ActivateBid = 7,
        /// <summary>
        /// The `change_bid_public_key` native entry point, used to change a bid's public key.
        /// </summary>
        ChangeBidPublicKey = 8,
        /// <summary>
        /// Used to call entry point call() in session transactions
        /// </summary>
        Call = 9,
    }
    
    public class TransactionEntryPoint
    {
        public NativeEntryPoint? Native { get; init; }
        
        public string Custom { get; init; }

        public TransactionEntryPoint(NativeEntryPoint name)
        {
            Native = name;
            Custom = null;
        }
        
        public TransactionEntryPoint(string customEntryPoint)
        {
            Native = null;
            Custom = customEntryPoint;
        }
        
        public class TransactionEntryPointConverter : JsonConverter<TransactionEntryPoint>
        {
            public override TransactionEntryPoint Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var nativeEntryPoint = EnumCompat.Parse<NativeEntryPoint>(reader.GetString());
                    return new TransactionEntryPoint(nativeEntryPoint);
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.PropertyName &&
                        reader.GetString() == "Custom")
                    {
                        reader.Read();
                        var customEntryPoint = reader.GetString();
                        reader.Read();
                        return new TransactionEntryPoint(customEntryPoint);
                    }
                }
                    
                throw new JsonException("Cannot deserialize TransactionEntryPoint.");
            }

            public override void Write(
                Utf8JsonWriter writer,
                TransactionEntryPoint value,
                JsonSerializerOptions options)
            {
                if (value.Native.HasValue)
                {
                    writer.WriteStringValue(value.Native.Value.ToString());
                } 
                else if (value.Custom != null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Custom");
                    writer.WriteStringValue(value.Custom);
                    writer.WriteEndObject();
                }
                else
                    throw new JsonException("Cannot serialize empty transaction entry point.");
            }
        }
    }
}
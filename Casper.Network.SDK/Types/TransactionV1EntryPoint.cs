using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Collection of entry points available for native calls to the system contracts.
    /// </summary>
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
        
        /// <summary>
        /// The `add_reservations` native entry point, used to add delegators to validator's reserve list.
        /// </summary>
        AddReservations = 10,
        
        /// <summary>
        /// The `cancel_reservations` native entry point, used to remove delegators from validator's reserve list.
        /// </summary>
        CancelReservations = 11,
    }


    public interface ITransactionV1EntryPoint
    {
        public string Name { get; }
        
        public byte[] ToBytes();
    }

    public class NativeTransactionV1EntryPoint : ITransactionV1EntryPoint
    {
        public NativeEntryPoint Type { get; init; }

        public string Name
        {
            get { return Type.ToString(); }
        }

        public NativeTransactionV1EntryPoint(NativeEntryPoint type)
        {
            Type = type;
        }

        public NativeTransactionV1EntryPoint(string name)
        {
            try
            {
                var nativeEntryPoint = EnumCompat.Parse<NativeEntryPoint>(name);
                Type = nativeEntryPoint;
            }
            catch (Exception e)
            {
                throw new Exception($"Invalid name for a TransactionV1NativeEntryPoint ({name}).");
            }
        }

        const ushort TAG_FIELD_INDEX = 0;
        const byte CALL_VARIANT_TAG = 1;
        const byte TRANSFER_VARIANT_TAG = 2;
        const byte ADD_BID_VARIANT_TAG = 3;
        const byte WITHDRAW_BID_VARIANT_TAG = 4;
        const byte DELEGATE_VARIANT_TAG = 5;
        const byte UNDELEGATE_VARIANT_TAG = 6;
        const byte REDELEGATE_VARIANT_TAG = 7;
        const byte ACTIVATE_BID_VARIANT_TAG = 8;
        const byte CHANGE_BID_PUBLIC_KEY_VARIANT_TAG = 9;
        const byte ADD_RESERVATIONS_VARIANT_TAG = 10;
        const byte CANCEL_RESERVATIONS_VARIANT_TAG = 11;
        
        public byte[] ToBytes()
        {
            var tag = Type switch
            {
                NativeEntryPoint.Call => CALL_VARIANT_TAG,
                NativeEntryPoint.Transfer => TRANSFER_VARIANT_TAG,
                NativeEntryPoint.AddBid => ADD_BID_VARIANT_TAG,
                NativeEntryPoint.Delegate => DELEGATE_VARIANT_TAG,
                NativeEntryPoint.WithdrawBid => WITHDRAW_BID_VARIANT_TAG,
                NativeEntryPoint.Undelegate => UNDELEGATE_VARIANT_TAG,
                NativeEntryPoint.Redelegate => REDELEGATE_VARIANT_TAG,
                NativeEntryPoint.ActivateBid => ACTIVATE_BID_VARIANT_TAG,
                NativeEntryPoint.ChangeBidPublicKey => CHANGE_BID_PUBLIC_KEY_VARIANT_TAG,
                NativeEntryPoint.AddReservations => ADD_RESERVATIONS_VARIANT_TAG,
                NativeEntryPoint.CancelReservations => CANCEL_RESERVATIONS_VARIANT_TAG,
                _ => throw new Exception($"Unknown NativeEntryPoint type: {Type}")
            };
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] {tag})
                .GetBytes();
        }
    }

    public class CustomTransactionV1EntryPoint : ITransactionV1EntryPoint
    {
        public string Name { get; init; }

        public CustomTransactionV1EntryPoint(string name)
        {
            Name = name;
        }
        
        const ushort TAG_FIELD_INDEX = 0;
        const byte CUSTOM_VARIANT_TAG = 1;
        const ushort CUSTOM_CUSTOM_INDEX = 1;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, new byte[] {CUSTOM_VARIANT_TAG})
                .AddField(CUSTOM_CUSTOM_INDEX, CLValue.String(Name))
                .GetBytes();
        }
    }

    /// <summary>
    /// A native or custom entry point to call within a transaction.
    /// </summary>
    public class TransactionV1EntryPoint
    {
        public static ITransactionV1EntryPoint Transfer => 
            new NativeTransactionV1EntryPoint(NativeEntryPoint.Transfer);

        public static ITransactionV1EntryPoint AddBid =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.AddBid);

        public static ITransactionV1EntryPoint WithdrawBid =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.WithdrawBid);

        public static ITransactionV1EntryPoint Delegate =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.Delegate);

        public static ITransactionV1EntryPoint Undelegate =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.Undelegate);

        public static ITransactionV1EntryPoint Redelegate =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.Redelegate);

        public static ITransactionV1EntryPoint ActivateBid =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.ActivateBid);

        public static ITransactionV1EntryPoint ChangeBidPublicKey =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.ChangeBidPublicKey);

        public static ITransactionV1EntryPoint Call =>
            new NativeTransactionV1EntryPoint(NativeEntryPoint.Call);

        public static ITransactionV1EntryPoint Custom(string name) => 
            new CustomTransactionV1EntryPoint(name);
        
        public class TransactionEntryPointConverter : JsonConverter<ITransactionV1EntryPoint>
        {
            public override ITransactionV1EntryPoint Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    return new NativeTransactionV1EntryPoint(reader.GetString());
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
                        return new CustomTransactionV1EntryPoint(customEntryPoint);
                    }
                }

                throw new JsonException("Cannot deserialize TransactionEntryPoint.");
            }

            public override void Write(
                Utf8JsonWriter writer,
                ITransactionV1EntryPoint value,
                JsonSerializerOptions options)
            {
                switch (value)
                {
                    case NativeTransactionV1EntryPoint nativeEntryPoint:
                        writer.WriteStringValue(nativeEntryPoint.Name);
                        break;
                    case CustomTransactionV1EntryPoint customEntryPoint:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Custom");
                        writer.WriteStringValue(customEntryPoint.Name);
                        writer.WriteEndObject();
                        break;
                    default:
                        throw new JsonException("Cannot serialize empty transaction entry point.");
                }
            }
        }
    }
}
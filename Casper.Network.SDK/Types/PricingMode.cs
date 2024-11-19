using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum PricingModeType
    {
        /// <summary>
        /// The original payment model, where the creator of the transaction specifies how much they will pay,
        /// at what gas price.
        /// </summary>
        PaymentLimited = 0,

        /// <summary>
        /// The cost of the transaction is determined by the cost table, per the transaction category.
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// The payment for this transaction was previously reserved, as proven by the receipt hash
        /// (this is for future use, not currently supported by the Casper network).
        /// </summary>
        Prepaid = 2,
    }

    public interface IPricingMode
    {
#if NET7_0_OR_GREATER
        public bool IsPaymentLimited => this is PaymentLimitedPricingMode;
        public bool IsFixed => this is FixedPricingMode;
        public bool IsReserved => this is PrepaidPricingMode;
#endif
        
        public byte[] ToBytes();
    }

    public class PaymentLimitedPricingMode : IPricingMode
    {
        /// <summary>
        /// Payment amount.
        /// </summary>
        [JsonPropertyName("payment_amount")]
        public ulong PaymentAmount { get; init; }

        /// <summary>
        /// Standard payment.
        /// </summary>
        [JsonPropertyName("standard_payment")]
        public bool StandardPayment { get; init; }

        /// <summary>
        /// User-specified gas_price tolerance (minimum 1). This is interpreted to mean "do not include this
        /// transaction in a block if the current gas price is greater than this number".
        /// </summary>
        [JsonPropertyName("gas_price_tolerance")]
        public byte GasPriceTolerance { get; init; }

        const ushort TAG_FIELD_INDEX = 0;
        const byte PAYMENT_LIMITED_VARIANT_TAG = 0;
        const ushort PAYMENT_LIMITED_PAYMENT_AMOUNT_INDEX = 1;
        const ushort PAYMENT_LIMITED_GAS_PRICE_TOLERANCE_INDEX = 2;
        const ushort PAYMENT_LIMITED_STANDARD_PAYMENT_INDEX = 3;

        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, CLValue.U8(PAYMENT_LIMITED_VARIANT_TAG))
                .AddField(PAYMENT_LIMITED_PAYMENT_AMOUNT_INDEX, CLValue.U64(PaymentAmount))
                .AddField(PAYMENT_LIMITED_GAS_PRICE_TOLERANCE_INDEX, CLValue.U8(GasPriceTolerance))
                .AddField(PAYMENT_LIMITED_STANDARD_PAYMENT_INDEX, CLValue.Bool(StandardPayment))
                .GetBytes();
        }
    }

    public class FixedPricingMode : IPricingMode
    {
        /// <summary>
        /// User-specified gas_price tolerance (minimum 1). This is interpreted to mean "do not include this
        /// transaction in a block if the current gas price is greater than this number".
        /// </summary>
        [JsonPropertyName("gas_price_tolerance")]
        public byte GasPriceTolerance { get; init; }
        
        /// <summary>
        /// User-specified additional computation factor (minimum 0). If "0" is provided,
        ///  no additional logic is applied to the computation limit. Each value above "0"
        ///  tells the node that it needs to treat the transaction as if it uses more gas
        ///  than it's serialized size indicates. Each "1" will increase the "wasm lane"
        ///  size bucket for this transaction by 1. So if the size of the transaction
        ///  indicates bucket "0" and "additional_computation_factor = 2", the transaction
        ///  will be treated as a "2".
        /// </summary>
        [JsonPropertyName("additional_computation_factor")]
        public byte AdditionalComputationFactor { get; init; }
        
        const ushort TAG_FIELD_INDEX = 0;
        const byte FIXED_VARIANT_TAG = 1;
        const ushort FIXED_GAS_PRICE_TOLERANCE_INDEX = 1;
        const ushort FIXED_ADDITIONAL_COMPUTATION_FACTOR_INDEX = 2;
        
        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, CLValue.U8(FIXED_VARIANT_TAG))
                .AddField(FIXED_GAS_PRICE_TOLERANCE_INDEX, CLValue.U8(GasPriceTolerance))
                .AddField(FIXED_ADDITIONAL_COMPUTATION_FACTOR_INDEX, CLValue.U8(AdditionalComputationFactor))
                .GetBytes();
        }
    }

    public class PrepaidPricingMode : IPricingMode
    {
        /// <summary>
        /// Pre-paid receipt in the Prepaid Pricing mode.
        /// </summary>
        [JsonPropertyName("receipt")]
        public string Receipt { get; init; }
        
        const ushort TAG_FIELD_INDEX = 0;
        const byte RESERVED_VARIANT_TAG = 2;
        const ushort RESERVED_RECEIPT_INDEX = 1;
        public byte[] ToBytes()
        {
            return new CalltableSerialization()
                .AddField(TAG_FIELD_INDEX, CLValue.U8(RESERVED_VARIANT_TAG))
                .AddField(RESERVED_RECEIPT_INDEX, Hex.Decode(Receipt))
                .GetBytes();
        }
    }

    /// <summary>
    /// Pricing mode of a Transaction.
    /// </summary>
    public class PricingMode
    {
        /// <summary>
        /// The original payment model, where the creator of the transaction
        /// specifies how much they will pay, at what gas price.
        /// </summary>
        /// <param name="paymentAmount">Amount in motes to pay for the transaction.</param>
        /// <param name="gasPriceTolerance">Defaults to 1. Gas price tolerance admitted.</param>
        /// <param name="standardPayment">Defaults to true. Indicates if this is a standar payment (non-standard payments are handled via wasm code).</param>
        public static IPricingMode PaymentLimited(ulong paymentAmount, byte gasPriceTolerance = 1, bool standardPayment = true)
        {
            return new PaymentLimitedPricingMode()
            {
                StandardPayment = standardPayment,
                GasPriceTolerance = gasPriceTolerance,
                PaymentAmount = paymentAmount,
            };
        }

        /// <summary>
        /// The cost of the transaction is determined by the cost table, per the transaction category.
        /// </summary>
        /// <param name="gasPriceTolerance">Defaults to 1. Gas price tolerance admitted.</param>
        /// <param name="additionalComputationFactor">Defaults to 0. Each unit increases the wasm lane the transaction is executed in..</param>
        public static IPricingMode Fixed(byte gasPriceTolerance = 1, byte additionalComputationFactor = 0)
        {
            return new FixedPricingMode()
            {
                GasPriceTolerance = gasPriceTolerance,
                AdditionalComputationFactor = additionalComputationFactor,
            };
        }

        /// <summary>
        /// The payment for this transaction was previously pre-paid, as proven by the receipt hash.
        /// </summary>
        /// <param name="receipt">Pre-paid receipt.</param>
        public static IPricingMode Prepaid(string receipt)
        {
            return new PrepaidPricingMode()
            {
                Receipt = receipt,
            };
        }

        public class PricingModeConverter : JsonConverter<IPricingMode>
        {
            public override IPricingMode Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                IPricingMode pricingMode = null;

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize PricingMode. StartObject expected");
                reader.Read();

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Cannot deserialize PricingMode. PropertyName expected");

                if (!EnumCompat.TryParse(reader.GetString(), out PricingModeType pricingModeType))
                    throw new Exception($"Unknown pricing mode '{reader.GetString()}'");

                reader.Read();

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize PricingMode. StartObject expected");

                switch (pricingModeType)
                {
                    case PricingModeType.PaymentLimited:
                        pricingMode = JsonSerializer.Deserialize<PaymentLimitedPricingMode>(ref reader, options);
                        break;
                    case PricingModeType.Fixed:
                        pricingMode = JsonSerializer.Deserialize<FixedPricingMode>(ref reader, options);
                        break;
                    case PricingModeType.Prepaid:
                        pricingMode = JsonSerializer.Deserialize<PrepaidPricingMode>(ref reader, options);
                        break;
                }

                reader.Read();

                return pricingMode;
            }

            public override void Write(
                Utf8JsonWriter writer,
                IPricingMode value,
                JsonSerializerOptions options)
            {
                switch (value)
                {
                    case PaymentLimitedPricingMode classicPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("PaymentLimited");
                        JsonSerializer.Serialize(writer, classicPricingMode);
                        writer.WriteEndObject();
                        break;
                    case FixedPricingMode fixedPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Fixed");
                        JsonSerializer.Serialize(writer, fixedPricingMode);
                        writer.WriteEndObject();
                        break;
                    case PrepaidPricingMode reservedPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Prepaid");
                        JsonSerializer.Serialize(writer, reservedPricingMode);
                        writer.WriteEndObject();
                        break;
                }
            }
        }
    }
}
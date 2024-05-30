using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
public enum PricingModeType
    {
        /// <summary>
        /// The original payment model, where the creator of the transaction specifies how much they will pay,
        /// at what gas price.
        /// </summary>
        Classic = 0,

        /// <summary>
        /// The cost of the transaction is determined by the cost table, per the transaction kind.
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// The payment for this transaction was previously reserved, as proven by the receipt hash
        /// (this is for future use, not currently supported by the Casper network).
        /// </summary>
        Reserved = 2,
    }

    /// <summary>
    /// Pricing mode of a Transaction.
    /// </summary>
    public class PricingMode
    {
        /// <summary>
        /// Pricing mode used: Classic, Fixed, Reserved.
        /// <see cref="PricingModeType"/>
        /// </summary>
        public PricingModeType Type { get; init; }

        /// <summary>
        /// Payment amount.
        /// </summary>
        public UInt64? PaymentAmount { get; set; }

        /// <summary>
        /// User-specified gas_price tolerance (minimum 1). This is interpreted to mean "do not include this
        /// transaction in a block if the current gas price is greater than this number".
        /// </summary>
        public UInt16? GasPriceTolerance { get; set; }

        /// <summary>
        /// Standard payment.
        /// </summary>
        public bool? StandardPayment { get; set; }

        /// <summary>
        /// Pre-paid receipt in the Reserved Pricing mode.
        /// </summary>
        public string Receipt { get; init; }

        public class PricingModeConverter : JsonConverter<PricingMode>
        {
            public override PricingMode Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize PricingMode. StartObject expected");
                reader.Read();

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Cannot deserialize PricingMode. PropertyName expected");

                string pricingModeType = reader.GetString();
                reader.Read();

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize PricingMode. StartObject expected");
                reader.Read();

                UInt64? paymentAmount = null;
                UInt16? gasPriceTolerance = null;
                bool? standardPayment = null;
                string receipt = null;

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var field = reader.GetString();
                    reader.Read();
                    switch (field)
                    {
                        case "payment_amount":
                            paymentAmount = reader.GetUInt64();
                            break;
                        case "standard_payment":
                            standardPayment = reader.GetBoolean();
                            break;
                        case "gas_price_tolerance":
                            gasPriceTolerance = reader.GetUInt16();
                            break;
                        case "receipt":
                            receipt = reader.GetString();
                            break;
                    }

                    reader.Read();
                }

                return new PricingMode()
                {
                    Type = EnumCompat.Parse<PricingModeType>(pricingModeType),
                    PaymentAmount = paymentAmount,
                    GasPriceTolerance = gasPriceTolerance,
                    StandardPayment = standardPayment,
                    Receipt = receipt,
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                PricingMode value,
                JsonSerializerOptions options)
            {
                if (value.Type == PricingModeType.Classic)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Classic");
                    writer.WriteStartObject();
                    if (value.PaymentAmount.HasValue)
                        writer.WriteNumber("payment_amount", value.PaymentAmount.Value);
                    if (value.GasPriceTolerance.HasValue)
                        writer.WriteNumber("gas_price_tolerance", value.GasPriceTolerance.Value);
                    if (value.StandardPayment.HasValue)
                        writer.WriteBoolean("standard_payment", value.StandardPayment.Value);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (value.Type == PricingModeType.Fixed)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Fixed");
                    writer.WriteStartObject();
                    if (value.GasPriceTolerance.HasValue)
                        writer.WriteNumber("gas_price_tolerance", value.GasPriceTolerance.Value);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (value.Type == PricingModeType.Reserved)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Reserved");
                    writer.WriteStartObject();
                    writer.WriteString("receipt", value.Receipt);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
            }
        }
    }
}
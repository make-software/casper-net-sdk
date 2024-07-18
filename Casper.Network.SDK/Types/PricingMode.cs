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
        /// The cost of the transaction is determined by the cost table, per the transaction category.
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
        public ulong PaymentAmount { get; set; }

        /// <summary>
        /// User-specified gas_price tolerance (minimum 1). This is interpreted to mean "do not include this
        /// transaction in a block if the current gas price is greater than this number".
        /// </summary>
        public byte GasPriceTolerance { get; set; }

        /// <summary>
        /// Standard payment.
        /// </summary>
        public bool StandardPayment { get; set; }

        /// <summary>
        /// Pre-paid receipt in the Reserved Pricing mode.
        /// </summary>
        public string Receipt { get; init; }

        /// <summary>
        /// The original payment model, where the creator of the transaction
        /// specifies how much they will pay, at what gas price.
        /// </summary>
        /// <param name="paymentAmount">Amount in motes to pay for the transaction.</param>
        /// <param name="gasPriceTolerance">Defaults to 1. Gas price tolerance admitted.</param>
        /// <param name="standardPayment">Defaults to true. Indicates if this is a standar payment (non-standard payments are handled via wasm code).</param>
        public static PricingMode Classic(ulong paymentAmount, byte gasPriceTolerance = 1, bool standardPayment = true)
        {
            return new PricingMode()
            {
                Type = PricingModeType.Classic,
                StandardPayment = standardPayment,
                GasPriceTolerance = gasPriceTolerance,
                PaymentAmount = paymentAmount,
            };
        }

        /// <summary>
        /// The cost of the transaction is determined by the cost table, per the transaction category.
        /// </summary>
        /// <param name="gasPriceTolerance">Defaults to 1. Gas price tolerance admitted.</param>
        public static PricingMode Fixed(byte gasPriceTolerance = 1)
        {
            return new PricingMode()
            {
                Type = PricingModeType.Fixed,
                GasPriceTolerance = gasPriceTolerance,
            };
        }

        /// <summary>
        /// The payment for this transaction was previously reserved, as proven by the receipt hash.
        /// </summary>
        /// <param name="receipt">Pre-paid receipt.</param>
        public static PricingMode Reserved(string receipt)
        {
            return new PricingMode()
            {
                Type = PricingModeType.Reserved,
                Receipt = receipt,
            };
        }
        
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

                ulong paymentAmount = 0;
                byte gasPriceTolerance = 0;
                bool standardPayment = false;
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
                            gasPriceTolerance = reader.GetByte();
                            break;
                        case "receipt":
                            receipt = reader.GetString();
                            break;
                    }

                    reader.Read();
                }

                reader.Read();
                
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
                    writer.WriteNumber("payment_amount", value.PaymentAmount);
                    writer.WriteNumber("gas_price_tolerance", value.GasPriceTolerance);
                    writer.WriteBoolean("standard_payment", value.StandardPayment);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (value.Type == PricingModeType.Fixed)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Fixed");
                    writer.WriteStartObject();
                    writer.WriteNumber("gas_price_tolerance", value.GasPriceTolerance);
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
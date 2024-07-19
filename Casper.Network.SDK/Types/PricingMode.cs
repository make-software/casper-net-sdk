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

    public interface IPricingMode
    {
#if NET7_0_OR_GREATER
        public bool IsClassic => this is ClassicPricingMode;
        public bool IsFixed => this is FixedPricingMode;
        public bool IsReserved => this is ReservedPricingMode;
#endif
    }
    
    public class ClassicPricingMode: IPricingMode
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
    }
    
    public class FixedPricingMode: IPricingMode
    {
        /// <summary>
        /// User-specified gas_price tolerance (minimum 1). This is interpreted to mean "do not include this
        /// transaction in a block if the current gas price is greater than this number".
        /// </summary>
        [JsonPropertyName("gas_price_tolerance")]
        public byte GasPriceTolerance { get; init; }
    }
    
    public class ReservedPricingMode: IPricingMode
    {
        /// <summary>
        /// Pre-paid receipt in the Reserved Pricing mode.
        /// </summary>
        [JsonPropertyName("receipt")]
        public string Receipt { get; init; }
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
        public static IPricingMode Classic(ulong paymentAmount, byte gasPriceTolerance = 1, bool standardPayment = true)
        {
            return new ClassicPricingMode()
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
        public static IPricingMode Fixed(byte gasPriceTolerance = 1)
        {
            return new FixedPricingMode()
            {
                GasPriceTolerance = gasPriceTolerance,
            };
        }

        /// <summary>
        /// The payment for this transaction was previously reserved, as proven by the receipt hash.
        /// </summary>
        /// <param name="receipt">Pre-paid receipt.</param>
        public static IPricingMode Reserved(string receipt)
        {
            return new ReservedPricingMode()
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

                if(!EnumCompat.TryParse(reader.GetString(), out PricingModeType pricingModeType))
                    throw new Exception($"Unknown pricing mode '{reader.GetString()}'");

                reader.Read();

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Cannot deserialize PricingMode. StartObject expected");
                
                switch (pricingModeType)
                {
                    case PricingModeType.Classic:
                        pricingMode = JsonSerializer.Deserialize<ClassicPricingMode>(ref reader, options);
                        break;
                    case PricingModeType.Fixed:
                        pricingMode = JsonSerializer.Deserialize<FixedPricingMode>(ref reader, options);
                        break;
                    case PricingModeType.Reserved:
                        pricingMode = JsonSerializer.Deserialize<ReservedPricingMode>(ref reader, options);
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
                    case ClassicPricingMode classicPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Classic");
                        JsonSerializer.Serialize(writer, classicPricingMode);
                        writer.WriteEndObject();
                        break;
                    case FixedPricingMode fixedPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Fixed");
                        JsonSerializer.Serialize(writer, fixedPricingMode);
                        writer.WriteEndObject();
                        break;
                    case ReservedPricingMode reservedPricingMode:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Reserved");
                        JsonSerializer.Serialize(writer, reservedPricingMode);
                        writer.WriteEndObject();
                        break;
                }
            }
        }
    }
}
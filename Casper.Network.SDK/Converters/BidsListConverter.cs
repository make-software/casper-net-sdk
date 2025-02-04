using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Converters
{
    public class BidKindsListConverter : JsonConverter<List<BidKind>>
    {
        public override List<BidKind> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var bids = new List<BidKind>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("StartArray token expected to deserialize a list of Bids");

            reader.Read(); // Start array

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();

                string publicKey = null;
                BidKind bid = null;

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();
                    switch (property)
                    {
                        case "public_key":
                            publicKey = reader.GetString();
                            reader.Read();
                            break;
                        case "bid":
                            try
                            {
                                using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                                {
                                    if (document.RootElement.TryGetProperty("bonding_purse", out var bondingPurse))
                                    {
                                        var unifiedBid =
                                            JsonSerializer.Deserialize<Bid>(document.RootElement.GetRawText());
                                        bid = new BidKind()
                                        {
                                            Unified = unifiedBid,
                                        };
                                    }
                                    else
                                    {
                                        bid = JsonSerializer.Deserialize<BidKind>(document.RootElement.GetRawText());
                                    }

                                    reader.Read(); // read end of object
                                }
                            }
                            catch (Exception e)
                            {
                                throw new JsonException(e.Message);
                            }

                            break;
                        default:
                            throw new JsonException($"Unexpected property '{property}' deserializing Bid");
                    }
                }

                if (publicKey != null && bid != null && bid.Unified != null && bid.Unified.PublicKey == null)
                    bid = new BidKind()
                    {
                        Unified = new Bid()
                        {
                            BondingPurse = bid.Unified.BondingPurse,
                            DelegationRate = bid.Unified.DelegationRate,
                            Delegators = bid.Unified.Delegators,
                            Inactive = bid.Unified.Inactive,
                            StakedAmount = bid.Unified.StakedAmount,
                            PublicKey = PublicKey.FromHexString(publicKey),
                            VestingSchedule = bid.Unified.VestingSchedule,
                        }
                    };
                bids.Add(bid);

                if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException("End object token expected while deserializing a list of Bids");
                reader.Read(); // end object
            }

            return bids;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<BidKind> value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException("Write method for Bid not yet implemented");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Converters
{
    public class BidsListConverter : JsonConverter<List<Bid>>
    {
        public override List<Bid> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var bids = new List<Bid>();
            
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("StartArray token expected to deserialize a list of Bids");

            reader.Read(); // Start array

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();
                
                string publicKey = null;
                Bid bid = null;
                
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
                            bid = JsonSerializer.Deserialize<Bid>(ref reader, options);
                            reader.Read(); // end object
                            break;
                        default:
                            throw new JsonException($"Unexpected property '{property}' deserializing Bid");
                    }
                }

                if (bid != null && bid.PublicKey == null && publicKey != null)
                    bid = new Bid
                    {
                        BondingPurse = bid.BondingPurse,
                        DelegationRate = bid.DelegationRate,
                        Delegators = bid.Delegators,
                        Inactive = bid.Inactive,
                        StakedAmount = bid.StakedAmount,
                        PublicKey = PublicKey.FromHexString(publicKey),
                        VestingSchedule = bid.VestingSchedule,
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
            List<Bid> value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException("Write method for Bid not yet implemented");
        }
    }
}
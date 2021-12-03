using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    public class VestingSchedule
    {
        [JsonPropertyName("initial_release_timestamp_millis")]
        public ulong InitialReleaseTimestampMillis { get; init; }

        [JsonPropertyName("locked_amounts")] 
        [JsonConverter(typeof(GenericListConverter<BigInteger,BigIntegerConverter>))]
        public List<BigInteger> LockedAmounts { get; init; }
    }
}

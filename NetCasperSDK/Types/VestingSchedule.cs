using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    public class VestingSchedule
    {
        [JsonPropertyName("initial_release_timestamp_millis")]
        public ulong InitialReleaseTimestampMillis { get; init; }

        [JsonPropertyName("locked_amounts")] 
        [JsonConverter(typeof(BigIntegerConverter))]
        public List<BigInteger> LockedAmounts { get; init; }
    }
}

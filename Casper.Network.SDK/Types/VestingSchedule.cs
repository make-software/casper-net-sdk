using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Vesting schedule for a genesis validator. `None` if non-genesis validator.
    /// </summary>
    public class VestingSchedule
    {
        [JsonPropertyName("initial_release_timestamp_millis")]
        public ulong InitialReleaseTimestampMillis { get; init; }

        [JsonPropertyName("locked_amounts")] 
        [JsonConverter(typeof(GenericListConverter<BigInteger,BigIntegerConverter>))]
        public List<BigInteger> LockedAmounts { get; init; }
    }
}

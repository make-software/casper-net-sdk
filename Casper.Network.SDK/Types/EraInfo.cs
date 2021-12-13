using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Auction metadata. Intended to be recorded at each era.
    /// </summary>
    public class EraInfo
    {
        /// <summary>
        /// List of rewards allocated to delegators and validators.
        /// </summary>
        [JsonPropertyName("seigniorage_allocations")]
        [JsonConverter(typeof(GenericListConverter<SeigniorageAllocation,SeigniorageAllocation.SeigniorageAllocationConverter>))]
        public List<SeigniorageAllocation> SeigniorageAllocations { get; init; }
    }
}
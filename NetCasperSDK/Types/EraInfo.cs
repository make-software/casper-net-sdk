using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;

namespace NetCasperSDK.Types
{
    /// <summary>
    /// Auction metadata. Intended to be recorded at each era.
    /// </summary>
    public class EraInfo
    {
        [JsonPropertyName("seigniorage_allocations")]
        [JsonConverter(typeof(GenericListConverter<SeigniorageAllocation,SeigniorageAllocation.SeigniorageAllocationConverter>))]
        public List<SeigniorageAllocation> SeigniorageAllocations { get; init; }
    }
}
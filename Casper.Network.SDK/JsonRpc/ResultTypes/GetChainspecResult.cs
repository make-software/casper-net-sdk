using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    public class GetChainspecResult
    {
        /// <summary>
        /// The account returned.
        /// </summary>
        [JsonPropertyName("chainspec_bytes")]
        public ChainspecRawBytes ChainspecBytes { get; init; }
    }
}

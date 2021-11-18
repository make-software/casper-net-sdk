using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_auction_info" RPC response.
    /// </summary>
    public class GetAuctionInfoResult : RpcResult
    {
        /// <summary>
        /// The auction state.
        /// </summary>
        [JsonPropertyName("auction_state")]
        public AuctionState AuctionState { get; init; }
    }
}
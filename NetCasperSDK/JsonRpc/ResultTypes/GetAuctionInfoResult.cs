using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
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
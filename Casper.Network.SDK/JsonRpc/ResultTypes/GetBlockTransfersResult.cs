using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "chain_get_block_transfers" RPC response.
    /// </summary>
    public class GetBlockTransfersResult : RpcResult
    {
        /// <summary>
        /// The block hash
        /// </summary>
        [JsonPropertyName("block_hash")]
        public string BlockHash { get; init; }
        
        /// <summary>
        /// The block's transfers
        /// </summary>
        [JsonPropertyName("transfers")]
        [JsonConverter(typeof(GenericListConverter<ITransfer, Transfer.TransferConverter>))]
        public List<ITransfer> Transfers { get; init; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Types;

namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for the "info_get_validator_changes" RPC.
    /// </summary>
    public class GetValidatorChangesResult : RpcResult
    {
        /// <summary>
        /// A list of validators with changes.
        /// </summary>
        [JsonPropertyName("changes")]
        public List<ValidatorChanges> Changes { get; init; }
    }
}
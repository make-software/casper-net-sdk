using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetCasperSDK.Converters;
using NetCasperSDK.Types;


namespace NetCasperSDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "state_get_dictionary_item" RPC response.
    /// </summary>
    public class GetDictionaryItemResult : RpcResult
    {
        /// <summary>
        /// The key under which the value is stored.
        /// </summary>
        [JsonPropertyName("dictionary_key")]
        public string DictionaryKey { get; init; }        
        
        /// <summary>
        /// The stored value.
        /// </summary>
        [JsonPropertyName("stored_value")]
        [JsonConverter(typeof(StoredValue.StoredValueConverter))]
        public StoredValue StoredValue { get; init; }
        
        /// <summary>
        /// The merkle proof.
        /// </summary>
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }
    }
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.JsonRpc
{
    /// <summary>
    /// Error data in an RPC Response.
    /// </summary>
    public class RpcError
    {
        [JsonPropertyName("code")] public int Code { get; init; }
        
        [JsonPropertyName("message")] public string Message { get; init; }
        
        [JsonPropertyName("data")] public JsonElement Data { get; init; }

        public override string ToString()
        {
            var msg = $"[Code: {Code}] {Message}.";
            
            if (Data.ValueKind != JsonValueKind.Null)
                msg = msg + Environment.NewLine + "Data: " + Data.GetRawText();
            
            return msg;
        }
    }
}
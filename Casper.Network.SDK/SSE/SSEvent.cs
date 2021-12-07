using System;
using System.Text.Json;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    public class SSEvent
    {
        public EventType EventType { get; init; }

        public int Id { get; init; }

        public JsonElement Result { get; init; }

        public T Parse<T>()
        {
            string json = null;

            if (typeof(T) == typeof(BlockAdded))
                json = this.Result.GetProperty("BlockAdded").GetRawText();

            if (typeof(T) == typeof(DeployAccepted))
                json = this.Result.GetProperty("DeployAccepted").GetRawText();

            if (typeof(T) == typeof(DeployExpired))
                json = this.Result.GetProperty("DeployExpired").GetRawText();

            if (typeof(T) == typeof(DeployProcessed))
                json = this.Result.GetProperty("DeployProcessed").GetRawText();
            
            if (typeof(T) == typeof(Fault))
                json = this.Result.GetProperty("Fault").GetRawText();

            if (typeof(T) == typeof(FinalitySignature))
                json = this.Result.GetProperty("FinalitySignature").GetRawText();

            if (typeof(T) == typeof(Step))
                json = this.Result.GetProperty("Step").GetRawText();

            if (json is null)
                throw new Exception($"Type '{typeof(T)} not compatible with SSE event returned object.");
            
            return JsonSerializer.Deserialize<T>(json);
        }   
    }
}
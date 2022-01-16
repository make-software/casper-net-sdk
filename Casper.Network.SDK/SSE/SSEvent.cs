using System;
using System.Text.Json;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    /// <summary>
    /// An occurrence of a server side event delivered via callback method
    /// </summary>
    public class SSEvent
    {
        /// <summary>
        /// The type of the event.
        /// </summary>
        public EventType EventType { get; init; }

        /// <summary>
        /// The incremental id number of the event from the node.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// JSON object before parsing.
        /// Call the <see cref="Parse{T}"/> method to get a typed object with the event.
        /// </summary>
        public JsonElement Result { get; init; }

        /// <summary>
        /// Parses the json event and returns a typed object with the data.
        /// If T is not the correct type for the event this method throws an exception.
        /// </summary>
        /// <typeparam name="T">One of BlockAdded, DeployAccepted, DeployExpired, DeployProcessed,
        /// Fault, FinalitySignature or Step.</typeparam>
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
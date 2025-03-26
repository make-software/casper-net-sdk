using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    internal class StepCompat
    {
        /// <summary>
        /// The era in which the change occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The resulting operations (only for Casper v1.x).
        /// </summary>
        [JsonPropertyName("execution_effect")]
        public ExecutionEffect ExecutionEffect { get; init; }
        
        /// <summary>
        /// The effect of executing the deploy.
        /// </summary>
        [JsonPropertyName("execution_effects")]
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Effects { get; init; }
    }
    
    /// <summary>
    /// The journal of execution transforms from an Era
    /// </summary>
    [JsonConverter(typeof(StepConverter))]
    public class Step
    {
        /// <summary>
        /// The era in which the change occurred.
        /// </summary>
        [JsonPropertyName("era_id")]
        public ulong EraId { get; init; }
        
        /// <summary>
        /// The resulting operations (only for Casper v1.x).
        /// </summary>
        [JsonPropertyName("operations")]
        public List<Operation> Operations { get; init; }
        
        /// <summary>
        /// The effect of executing the deploy.
        /// </summary>
        [JsonPropertyName("execution_effects")]
        [JsonConverter(typeof(GenericListConverter<Transform, Transform.TransformConverter>))]
        public List<Transform> Effects { get; init; }
        
        public class StepConverter : JsonConverter<Step>
        {
            public override Step Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var stepCompat = JsonSerializer.Deserialize<StepCompat>(ref reader, options);

                if (stepCompat.ExecutionEffect == null)
                    return new Step()
                    {
                        EraId = stepCompat.EraId,
                        Effects = stepCompat.Effects,
                    };

                return new Step()
                {
                    EraId = stepCompat.EraId,
                    Operations = stepCompat.ExecutionEffect.Operations,
                    Effects = stepCompat.ExecutionEffect.Transforms.Select(t => (Transform)t).ToList(),
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                Step value,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("era_id", value.EraId);
                if (value.Operations != null && value.Operations.Count > 0)
                {
                    writer.WritePropertyName("operations");
                    JsonSerializer.Serialize(writer, value.Operations);
                }
                writer.WritePropertyName("execution_effects");

                if (value.Effects == null || value.Effects.Count == 0)
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
                else
                {
                    JsonSerializer.Serialize(writer, value.Effects);
                }
                writer.WriteEndObject();
            }
        }
    }
}
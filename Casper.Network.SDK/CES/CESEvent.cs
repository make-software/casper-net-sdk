using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.CES
{
    /// <summary>
    /// A fully parsed CES event containing the event name, its typed fields, and the
    /// execution-context metadata set by <see cref="CESParser.GetEvents"/>.
    /// </summary>
    public class CESEvent
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("fields")]
        [JsonConverter(typeof(NamedArgListConverter))]
        public IReadOnlyList<NamedArg> Fields { get; }

        /// <summary>
        /// The contract hash of the emitting contract (e.g. <c>"hash-abc…def"</c>),
        /// propagated from the <see cref="CESContractSchema"/> supplied to the parser.
        /// <c>null</c> when the schema was not annotated with a contract hash.
        /// </summary>
        [JsonPropertyName("contract_hash")]
        public string ContractHash { get; init; }

        /// <summary>
        /// The contract-package hash of the emitting contract,
        /// propagated from the <see cref="CESContractSchema"/> supplied to the parser.
        /// <c>null</c> when the schema was not annotated with a contract-package hash.
        /// </summary>
        [JsonPropertyName("contract_package_hash")]
        public string ContractPackageHash { get; init; }

        /// <summary>
        /// Zero-based index of the <see cref="Transform"/> inside the execution-result
        /// effect list from which this event was extracted.
        /// Set by <see cref="CESParser.GetEvents"/>; defaults to <c>0</c> when the event
        /// is parsed in isolation via <see cref="ParseEvent"/>.
        /// </summary>
        [JsonPropertyName("transform_id")]
        public int TransformId { get; init; }

        /// <summary>
        /// The string key that identifies this entry inside the contract's <c>__events</c>
        /// dictionary (i.e. the sequential event counter emitted by the contract).
        /// Set by <see cref="CESParser.GetEvents"/>; <c>null</c> when parsed in isolation.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; init; }

        [JsonConstructor]
        public CESEvent(string name, IReadOnlyList<NamedArg> fields)
        {
            Name = name;
            Fields = fields;
        }

        /// <summary>
        /// Returns the field with the given name, or <c>null</c> if not found.
        /// </summary>
        public NamedArg this[string fieldName] =>
            Fields.FirstOrDefault(f => f.Name == fieldName);

        // ─────────────────────────────────────────────────────────────────────
        // Parse
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a single CES event from its raw bytes using the given contract schema.
        /// </summary>
        /// <param name="rawBytes">
        /// The raw bytes of one event entry (value) from the <c>__events</c> CLValue map.
        /// </param>
        /// <param name="schema">
        /// The contract schema obtained from <see cref="CESContractSchema.ParseSchema"/>.
        /// </param>
        /// <param name="transformId">
        /// Zero-based index of the source <see cref="Transform"/> in the execution-result
        /// effect list.  Used to populate <see cref="TransformId"/>.
        /// Pass <c>0</c> (the default) when parsing in isolation.
        /// </param>
        /// <param name="eventId">
        /// The string key that identifies this entry in the contract's <c>__events</c>
        /// dictionary.  Used to populate <see cref="EventId"/>.
        /// Pass <c>null</c> (the default) when parsing in isolation.
        /// </param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the event name found in <paramref name="rawBytes"/> is not present in
        /// <paramref name="schema"/>.
        /// </exception>
        public static CESEvent ParseEvent(byte[] rawBytes, CESContractSchema schema,
            int transformId = 0, string eventId = null)
        {
            // Some CES implementations wrap the event payload in a Casper Vec<u8> (Bytes),
            // which prepends a u32 LE length equal to the remaining byte count. Detect and
            // skip that outer wrapper transparently.
            var offset = 0;
            if (rawBytes.Length >= 4)
            {
                var declaredLen = BitConverter.ToUInt32(rawBytes, 0);
                if (!BitConverter.IsLittleEndian)
                    declaredLen = SwapU32(declaredLen);
                if (declaredLen == rawBytes.Length - 4)
                    offset = 4;
            }

            using var ms = new MemoryStream(rawBytes, offset, rawBytes.Length - offset);
            using var reader = new BinaryReader(ms);

            var rawName = reader.ReadCLString();

            // Strip the "event_" prefix that some CES implementations prepend to event names.
            const string prefix = "event_";
            var eventName = rawName.StartsWith(prefix)
                ? rawName.Substring(prefix.Length)
                : rawName;

            if (!schema.TryGetEventSchema(eventName, out var eventSchema))
                throw new KeyNotFoundException(
                    $"Event '{rawName}' not found in the contract schema.");

            var fields = new List<NamedArg>(eventSchema.Fields.Count);
            var clValueReader = new CLValueReader(reader);

            foreach (var schemaField in eventSchema.Fields)
            {
                var clValue = clValueReader.Read(schemaField.CLTypeInfo);
                fields.Add(new NamedArg(schemaField.Name, clValue));
            }

            return new CESEvent(eventName, fields)
            {
                ContractHash = schema.ContractHash,
                ContractPackageHash = schema.ContractPackageHash,
                TransformId = transformId,
                EventId = eventId,
            };
        }

        private static uint SwapU32(uint value) =>
            ((value & 0x000000FFu) << 24) |
            ((value & 0x0000FF00u) <<  8) |
            ((value & 0x00FF0000u) >>  8) |
            ((value & 0xFF000000u) >> 24);

        // ─────────────────────────────────────────────────────────────────────
        // JSON converter for IReadOnlyList<NamedArg>
        //
        // Each NamedArg is serialised by NamedArg.NamedArgConverter as the
        // two-element JSON array ["fieldName", {CLValue}], so the field list
        // becomes an array of such pairs:
        //   [["amount", {...}], ["sender", {...}], ...]
        // ─────────────────────────────────────────────────────────────────────

        public class NamedArgListConverter : JsonConverter<IReadOnlyList<NamedArg>>
        {
            private static readonly NamedArg.NamedArgConverter _itemConverter =
                new NamedArg.NamedArgConverter();

            public override IReadOnlyList<NamedArg> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException("Expected start of array for CESEvent fields.");

                reader.Read(); // move past the outer '[' to the first element (or ']')

                var list = new List<NamedArg>();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    // Each element is itself a two-element array ["name", {CLValue}];
                    // NamedArgConverter.Read expects the reader to be at StartArray.
                    list.Add(_itemConverter.Read(ref reader, typeof(NamedArg), options));
                    reader.Read(); // advance past the EndArray of the just-read NamedArg
                }

                return list;
            }

            public override void Write(
                Utf8JsonWriter writer,
                IReadOnlyList<NamedArg> value,
                JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                foreach (var arg in value)
                    _itemConverter.Write(writer, arg, options);
                writer.WriteEndArray();
            }
        }
    }
}

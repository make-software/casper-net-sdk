using System.Collections.Generic;
using System.Linq;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.CES
{
    /// <summary>
    /// Defines one field in a CES event schema: its name and Casper type.
    /// </summary>
    public class CESEventSchemaField
    {
        public string Name { get; }
        public CLTypeInfo CLTypeInfo { get; }

        public CESEventSchemaField(string name, CLTypeInfo clTypeInfo)
        {
            Name = name;
            CLTypeInfo = clTypeInfo;
        }
    }

    /// <summary>
    /// Schema for a single named CES event type, listing its fields in order.
    /// </summary>
    public class CESEventSchema
    {
        public string EventName { get; }
        public IReadOnlyList<CESEventSchemaField> Fields { get; }

        public CESEventSchema(string eventName, IReadOnlyList<CESEventSchemaField> fields)
        {
            EventName = eventName;
            Fields = fields;
        }
    }

    /// <summary>
    /// Full schema for a CES-compliant contract, parsed from the <c>__events_schema</c> named key.
    /// Maps event names to their <see cref="CESEventSchema"/>.
    /// </summary>
    public class CESContractSchema
    {
        public IReadOnlyDictionary<string, CESEventSchema> Events { get; }

        public CESContractSchema(IReadOnlyDictionary<string, CESEventSchema> events)
        {
            Events = events;
        }
        
        /// <summary>
        /// Retrieves the schema for the given event name.
        /// </summary>
        public CESEventSchema GetEventSchema(string eventName)
        {
            if (!TryGetEventSchema(eventName, out var schema))
                throw new KeyNotFoundException($"Event '{eventName}' not found in the contract schema.");
            return schema;
        }

        /// <summary>
        /// Tries to retrieve the schema for the given event name.
        /// </summary>
        public bool TryGetEventSchema(string eventName, out CESEventSchema schema)
        {
            return Events.TryGetValue(eventName, out schema);
        }
    }

    /// <summary>
    /// A fully parsed CES event containing the event name and its typed fields.
    /// </summary>
    public class CESEvent
    {
        public string Name { get; }
        public IReadOnlyList<NamedArg> Fields { get; }

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
    }
}

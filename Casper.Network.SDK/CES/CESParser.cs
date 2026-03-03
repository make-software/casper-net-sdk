using System;
using System.Collections.Generic;
using System.Linq;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.CES
{
    /// <summary>
    /// Parses CES (Casper Event Standard) schema and event bytes produced by
    /// smart contracts following the make-software CES convention.
    /// <para>
    /// Contracts store their event schema in the named key <c>__events_schema</c>
    /// (a <c>CLValue</c> of type <c>Any</c>) and emit events into <c>__events</c>
    /// (a <c>CLValue</c> of type <c>Map(U32, Bytes)</c>).
    /// </para>
    /// </summary>
    public static class CESParser
    {
        /// <summary>
        /// Scans a list of execution-result transforms and returns every CES event emitted by
        /// any of the watched contracts.
        /// </summary>
        /// <param name="transforms">
        /// The <c>Effect</c> list from an <see cref="ExecutionResult"/> (V1 or V2).
        /// </param>
        /// <param name="watchedContracts">
        /// The list of <see cref="CESContractSchema"/> instances to watch.
        /// Each schema must have <see cref="CESContractSchema.EventsURef"/> set; schemas
        /// where it is <c>null</c> are silently skipped during matching.
        /// </param>
        /// <returns>
        /// Ordered list of <see cref="CESEvent"/> instances found, in the same order as they
        /// appear in <paramref name="transforms"/>.  Never null; empty when no events are found.
        /// </returns>
        public static List<CESEvent> GetEvents(
            List<Transform> transforms,
            IReadOnlyList<CESContractSchema> watchedContracts)
        {
            if (transforms == null)
                throw new ArgumentNullException(nameof(transforms));
            if (watchedContracts == null)
                throw new ArgumentNullException(nameof(watchedContracts));

            var results = new List<CESEvent>();

            for (int i = 0; i < transforms.Count; i++)
            {
                var transform = transforms[i];

                // 1. Key must be a dictionary entry.
                if (transform.Key is not DictionaryKey)
                    continue;

                // 2. Kind must be a CLValue write.
                if (transform.Kind is not WriteTransformKind writeKind)
                    continue;

                var clValue = writeKind.Value?.CLValue;
                if (clValue == null)
                    continue;

                // 3. Parse the dictionary envelope.
                CLValueDictionary dict;
                try
                {
                    dict = CLValueDictionary.Parse(clValue.Bytes);
                }
                catch
                {
                    // Not a CES dictionary entry — skip.
                    continue;
                }

                // 4. Match the seed address against the watched contracts.
                var seedHex = dict.Seed.ToHexString();
                var schema = watchedContracts.FirstOrDefault(s =>
                    s.EventsURef != null && s.EventsURef.ToHexString() == seedHex);
                if (schema == null)
                    continue;

                // 5. Parse the event payload, stamping it with execution-result context.
                try
                {
                    results.Add(CESEvent.ParseEvent(dict.Value, schema, i, dict.ItemKey));
                }
                catch (KeyNotFoundException)
                {
                    // Event name not present in this version of the schema — skip.
                }
            }

            return results;
        }
    }
}

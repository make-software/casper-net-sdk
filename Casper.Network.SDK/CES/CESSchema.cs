using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.CES
{
    /// <summary>
    /// Defines one field in a CES event schema: its name and Casper type.
    /// </summary>
    public class CESEventSchemaField
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("cl_type")]
        [JsonConverter(typeof(CLTypeInfoConverter))]
        public CLTypeInfo CLTypeInfo { get; }

        [JsonConstructor]
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
        [JsonPropertyName("event_name")]
        public string EventName { get; }

        [JsonPropertyName("fields")]
        public IReadOnlyList<CESEventSchemaField> Fields { get; }

        [JsonConstructor]
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
        [JsonPropertyName("events")]
        public IReadOnlyDictionary<string, CESEventSchema> Events { get; }

        /// <summary>
        /// The contract hash (e.g. <c>"hash-abc…def"</c> or <c>"entity-contract-abc…def"</c>)
        /// that owns this schema.
        /// Set by the caller after <see cref="ParseSchema"/>, or automatically
        /// populated when the schema is fetched via <see cref="LoadAsync"/>.
        /// </summary>
        [JsonPropertyName("contract_hash")]
        public string ContractHash { get; init; }

        /// <summary>
        /// The contract-package hash supplied to <see cref="LoadAsync"/> (or set manually
        /// after <see cref="ParseSchema"/>).
        /// </summary>
        [JsonPropertyName("contract_package_hash")]
        public string ContractPackageHash { get; init; }

        /// <summary>
        /// The URef stored under the <c>__events_schema</c> named key of the contract.
        /// Automatically populated by <see cref="LoadAsync"/>; <c>null</c> when the schema
        /// was obtained via <see cref="ParseSchema"/> directly.
        /// </summary>
        [JsonPropertyName("schema_uref")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef SchemaURef { get; init; }

        /// <summary>
        /// The URef stored under the <c>__events</c> named key of the contract.
        /// This is the seed used to locate CES event entries in the global state transforms.
        /// Automatically populated by <see cref="LoadAsync"/>; <c>null</c> when the schema
        /// was obtained via <see cref="ParseSchema"/> directly.
        /// </summary>
        [JsonPropertyName("events_uref")]
        [JsonConverter(typeof(GlobalStateKey.GlobalStateKeyConverter))]
        public URef EventsURef { get; init; }

        [JsonConstructor]
        public CESContractSchema(IReadOnlyDictionary<string, CESEventSchema> events)
        {
            Events = events;
        }

        /// <summary>
        /// Retrieves the schema for the given event name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <paramref name="eventName"/> is not present in this schema.
        /// </exception>
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

        // ─────────────────────────────────────────────────────────────────────
        // Parse
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the raw bytes of the <c>__events_schema</c> CLValue (type <c>Any</c>)
        /// into a <see cref="CESContractSchema"/>.
        /// </summary>
        /// <param name="rawBytes">
        /// The <c>Bytes</c> property of the CLValue retrieved from <c>__events_schema</c>.
        /// </param>
        public static CESContractSchema ParseSchema(byte[] rawBytes)
        {
            using var ms = new MemoryStream(rawBytes);
            using var reader = new BinaryReader(ms);

            var numEvents = reader.ReadInt32();
            var events = new Dictionary<string, CESEventSchema>(numEvents);

            for (int i = 0; i < numEvents; i++)
            {
                var eventName = reader.ReadCLString();

                var numFields = reader.ReadInt32();
                var fields = new List<CESEventSchemaField>(numFields);

                for (int j = 0; j < numFields; j++)
                {
                    var fieldName = reader.ReadCLString();
                    var fieldType = reader.ReadCLTypeInfo();
                    fields.Add(new CESEventSchemaField(fieldName, fieldType));
                }

                events[eventName] = new CESEventSchema(eventName, fields);
            }

            return new CESContractSchema(events);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Network factory
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches and parses the CES event schema for a contract from the Casper network.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method calls <c>GetPackage</c> to enumerate all active versions of the contract
        /// package and then resolves the target contract:
        /// </para>
        /// <list type="bullet">
        ///   <item>When <paramref name="version"/> is provided, that exact version is selected.</item>
        ///   <item>
        ///     When <paramref name="version"/> is <c>null</c> (the default), the highest-numbered
        ///     active (non-disabled) version is used.
        ///   </item>
        /// </list>
        /// <para>
        /// After resolving the contract the method fetches its named-key list to locate
        /// the <c>__events_schema</c> and <c>__events</c> URefs, then queries the schema
        /// CLValue directly from the <c>__events_schema</c> URef and parses it into a
        /// <see cref="CESContractSchema"/>.
        /// The resulting schema has <see cref="ContractHash"/>, <see cref="ContractPackageHash"/>,
        /// <see cref="SchemaURef"/>, and <see cref="EventsURef"/> automatically populated.
        /// </para>
        /// <para>
        /// Both the legacy (<c>ContractPackage</c>) and the new Casper-2.x
        /// (<c>Package</c> / entity) contract models are supported.
        /// </para>
        /// </remarks>
        /// <param name="client">An active <see cref="ICasperClient"/> instance.</param>
        /// <param name="contractPackageHash">
        /// The contract-package hash string, e.g.
        /// <c>"contract-package-hash-abc…def"</c> or <c>"package-abc…def"</c>.
        /// </param>
        /// <param name="version">
        /// The contract version number to load.  Pass <c>null</c> (the default) to load
        /// the latest active version.
        /// </param>
        /// <returns>
        /// A fully-populated <see cref="CESContractSchema"/> with
        /// <see cref="ContractHash"/> and <see cref="ContractPackageHash"/> set.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="client"/> or <paramref name="contractPackageHash"/> is null.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when a specific <paramref name="version"/> was requested but not found in the
        /// package.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the package has no active versions, the RPC result is in an unexpected
        /// format, or the <c>__events_schema</c> named key is absent.
        /// </exception>
        public static async Task<CESContractSchema> LoadAsync(
            ICasperClient client,
            string contractPackageHash,
            uint? version = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(contractPackageHash))
                throw new ArgumentNullException(nameof(contractPackageHash));

            // ── 1. Fetch the package to enumerate versions ────────────────────
            var pkgResult = (await client.QueryGlobalState(contractPackageHash)).Parse();

            // ── 2. Resolve the target contract version ────────────────────────
            string contractHash;

            if (pkgResult.StoredValue != null && 
                pkgResult.StoredValue.ContractPackage != null)
            {
                // Legacy node: Versions contains only active (non-disabled) entries.
                var pkg = pkgResult.StoredValue.ContractPackage;

                ContractVersion chosen;
                if (version.HasValue)
                {
                    chosen = pkg.Versions.FirstOrDefault(v => v.Version == version.Value)
                        ?? throw new KeyNotFoundException(
                            $"Version {version.Value} not found in contract package '{contractPackageHash}'.");
                }
                else
                {
                    chosen = pkg.Versions.OrderByDescending(v => v.Version).FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"Contract package '{contractPackageHash}' has no active versions.");
                }

                contractHash = chosen.Hash; // e.g. "contract-hash-abc…def"
            }
            else if (pkgResult.StoredValue != null && 
                     pkgResult.StoredValue.Package != null)
            {
                // New node: Versions contains all versions; filter out disabled ones for latest.
                var pkg = pkgResult.StoredValue.Package;
                var disabledSet = new HashSet<uint>(
                    pkg.DisabledVersions?.Select(d => d.Version) ?? Enumerable.Empty<uint>());

                EntityVersionAndHash chosen;
                if (version.HasValue)
                {
                    chosen = pkg.Versions.FirstOrDefault(v => v.EntityVersion.Version == version.Value)
                        ?? throw new KeyNotFoundException(
                            $"Version {version.Value} not found in contract package '{contractPackageHash}'.");
                }
                else
                {
                    chosen = pkg.Versions
                        .Where(v => !disabledSet.Contains(v.EntityVersion.Version))
                        .OrderByDescending(v => v.EntityVersion.Version)
                        .FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"Contract package '{contractPackageHash}' has no active versions.");
                }

                contractHash = chosen.AddressableEntity.ToString(); // e.g. "entity-contract-abc…def"
            }
            else
            {
                throw new InvalidOperationException(
                    $"GetPackage returned neither a ContractPackage nor a Package for '{contractPackageHash}'.");
            }

            var contractKey = GlobalStateKey.FromString(contractHash);

            // ── 3. Fetch named keys to locate __events_schema and __events ────
            List<NamedKey> namedKeys;
            if (contractKey is AddressableEntityKey)
            {
                // New node (Casper 2.x): named keys are returned by state_get_entity.
                var entityResult = (await client.GetEntity(contractHash)).Parse();
                namedKeys = entityResult.NamedKeys
                    ?? throw new InvalidOperationException(
                        $"No named keys returned for entity '{contractHash}'.");
            }
            else
            {
                // Legacy node: QueryGlobalState without a path returns the Contract
                // object, which includes its named-key list.
                var contractResult = (await client.QueryGlobalState(contractKey)).Parse();
                namedKeys = contractResult.StoredValue?.Contract?.NamedKeys
                    ?? throw new InvalidOperationException(
                        $"No named keys found for contract '{contractHash}'.");
            }

            var schemaNamedKey = namedKeys.FirstOrDefault(k => k.Name == "__events_schema")
                ?? throw new InvalidOperationException(
                    $"Named key '__events_schema' not found for contract '{contractHash}'.");

            var eventsNamedKey = namedKeys.FirstOrDefault(k => k.Name == "__events")
                ?? throw new InvalidOperationException(
                    $"Named key '__events' not found for contract '{contractHash}'.");

            var schemaURef = schemaNamedKey.Key as URef
                ?? throw new InvalidOperationException(
                    $"Expected a URef for '__events_schema' named key, got '{schemaNamedKey.Key?.GetType().Name}'.");

            var eventsURef = eventsNamedKey.Key as URef
                ?? throw new InvalidOperationException(
                    $"Expected a URef for '__events' named key, got '{eventsNamedKey.Key?.GetType().Name}'.");

            // ── 4. Query the schema CLValue directly from its URef ────────────
            var schemaResult = (await client.QueryGlobalState(schemaURef)).Parse();

            var clValue = schemaResult.StoredValue?.CLValue
                ?? throw new InvalidOperationException(
                    $"No CLValue at '__events_schema' URef for contract '{contractHash}'.");

            // ── 5. Parse and annotate ─────────────────────────────────────────
            var parsed = ParseSchema(clValue.Bytes);
            return new CESContractSchema(parsed.Events)
            {
                ContractHash = contractKey.ToString(),
                ContractPackageHash = contractPackageHash,
                SchemaURef = schemaURef,
                EventsURef = eventsURef,
            };
        }
    }
}

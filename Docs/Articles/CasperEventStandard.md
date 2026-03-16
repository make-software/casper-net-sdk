# Casper Event Standard (CES)

The **Casper Event Standard (CES)** is a convention adopted by contract developers for emitting and consuming typed events from Casper smart contracts. Contracts that follow CES store a self-describing schema alongside their events, making it possible to decode any event without out-of-band knowledge of its structure.

The Casper .NET SDK provides the `Casper.Network.SDK.CES` namespace with three classes: `CESContractSchema` (schema loading and parsing), `CESEvent` (event parsing and field access), and `CESParser` (scanning execution results).

---

## How CES Works

A CES-compliant contract uses two special named keys for events:

| Named key | Purpose |
|---|---|---|
| `__events_schema` | Binary-encoded schema listing every event type and its fields |
| `__events` | Ordered map of event index → raw event bytes |

### Schema (`__events_schema`)

The schema describes every event the contract can emit. It is stored as a CLValue of type `Any`, whose raw bytes encode a sequence of event descriptors:

```
u32 LE              — number of events
for each event:
  String            — event name (4-byte LE length + UTF-8 bytes)
  u32 LE            — number of fields
  for each field:
    String          — field name
    CLType bytes    — Casper binary type encoding (tag byte + optional inner types)
```

The `String` encoding used here is the standard Casper string encoding: a `u32` length followed by the UTF-8 content (no null terminator).

### Events (`__events`)

Each entry in the `__events` map is a `Bytes` (`Vec<u8>`) value. The raw bytes of one event entry are laid out as follows:

```
u32 LE              — Vec<u8> outer length (= remaining bytes; auto-detected and skipped)
String              — event name, prefixed with "event_" (e.g. "event_Mint")
for each field (in schema order):
  raw field bytes   — native Casper serialization, no CLValue wrapper
```

The `"event_"` prefix is stripped automatically when parsing the event raw bytes.

---

## Loading a Contract Schema from the Network

The simplest way to load a contract's schema is `CESContractSchema.LoadAsync`. It accepts either a **contract-package hash** or a direct **contract hash**, reads the named keys to locate `__events_schema` and `__events`, and fetches and parses the schema — all in a single call.

### Using a contract-package hash

When you pass a contract-package hash, the method resolves the latest active contract version automatically. You can also request a specific version with the optional `version` parameter.

```csharp
using Casper.Network.SDK.CES;

var client = new NetCasperClient("https://rpc.testnet.casperlabs.io/rpc");

// Load the schema for the latest active version of the contract package
CESContractSchema schema = await CESContractSchema.LoadAsync(
    client,
    "hash-17cb23ce7b6d663fc82bacbdd56a26dc722cabe7e69c84a3ca729bf3cb7fdc70");
```

### Using a contract hash directly

If you already know the contract hash, you can pass it directly and the package-resolution step is skipped entirely. Use `"contract-"` prefix instead of `"hash-"` for a contract hash argument.The `version` parameter is ignored in this case.

```csharp
CESContractSchema schema = await CESContractSchema.LoadAsync(
    client,
    "contract-dead1234dead1234dead1234dead1234dead1234dead1234dead1234dead1234beef");
```

The returned `CESContractSchema` is fully annotated:

| Property | Description                                                      |
|---|------------------------------------------------------------------|
| `Events` | Dictionary of event name → `CESEventSchema`                      |
| `ContractHash` | Hash of the resolved (or supplied) contract version              |
| `ContractPackageHash` | Package hash passed in. May be `null` when a direct contract hash was supplied.                                        |
| `SchemaURef` | URef of the `__events_schema` named key                          |
| `EventsURef` | URef of the `__events` named key (used when scanning transforms) |

Both legacy (Casper 1.x) and Casper 2.x contract models are supported automatically.

### Parsing a Schema from Raw Bytes

If you have already fetched the `__events_schema` CLValue from the network, you can parse it directly:

```csharp
byte[] schemaBytes = /* raw bytes from __events_schema CLValue */;
CESContractSchema schema = CESContractSchema.ParseSchema(schemaBytes);

// Inspect the event types the contract supports
foreach (var kvp in schema.Events)
{
    Console.WriteLine($"Event: {kvp.Key}");
    foreach (var field in kvp.Value.Fields)
        Console.WriteLine($"  {field.Name}: {field.CLTypeInfo.Type}");
}
```

Note that a schema created via `ParseSchema` directly has `SchemaURef` and `EventsURef` set to `null`. Use `LoadAsync` when you need these URefs.

---

## Parsing a Single Event

To decode one event payload, pass the raw event bytes together with a schema to `CESEvent.ParseEvent`:

```csharp
byte[] eventBytes = /* raw bytes of one entry from __events map */;
CESEvent evt = CESEvent.ParseEvent(eventBytes, schema);

Console.WriteLine(evt.Name); // e.g. "event_Mint"

// Access field values directly by name — returns CLValue, or null if not found
CLValue recipient = evt["recipient"];
Console.WriteLine(recipient.ToGlobalStateKey()); // hash-1262d06e...

CLValue amount = evt["amount"];
Console.WriteLine(amount.ToBigInteger()); // 1000000000000000000
```

The indexer `evt["fieldName"]` returns the `CLValue` of that field, or `null` if the field name is not found. The full list of fields is also available as `IReadOnlyList<NamedArg>` through `evt.Fields`, which preserves both name and value.

### Looking Up an Event Schema

```csharp
if (schema.TryGetEventSchema("Mint", out CESEventSchema mintSchema))
{
    Console.WriteLine($"Mint has {mintSchema.Fields.Count} fields.");
}
```

---

## Scanning Execution Results with `GetEvents`

`CESParser.GetEvents` scans the transform list of an execution result and returns all CES events emitted by any of the watched contracts:

```csharp
var events = CESParser.GetEvents(
    executionResult.Effect,
    new List<CESContractSchema> { schema1, schema2 });
```

For each transform the method:
1. Skips transforms whose key is not a `DictionaryKey`.
2. Skips transforms whose kind is not a `WriteTransformKind`.
3. Tries to parse the CLValue as a `CLValueDictionary`; skips on failure.
4. Matches the dictionary seed against each schema's `EventsURef` (access rights are ignored; only the 32-byte hash is compared).
5. Parses the matching event payload using the schema.
6. Silently skips events whose name is not present in the schema (graceful version mismatch handling).

Each schema in the list must have `EventsURef` set (schemas loaded via `LoadAsync` always have it). Schemas with a `null` `EventsURef` are silently skipped.

The returned `CESEvent` objects include execution-result context:

| Property | Description |
|---|---|
| `Name` | Raw event name as stored in bytes (e.g. `"event_Transfer"`) |
| `Fields` | Ordered list of `NamedArg` field values |
| `ContractHash` | Propagated from the matched schema |
| `ContractPackageHash` | Propagated from the matched schema |
| `TransformKey` | `GlobalStateKey` of the transform that emitted the event |
| `TransformId` | Zero-based index in the effect list |
| `EventId` | Sequential event counter key from `__events` |

---

## Data Model

```
CESContractSchema
  ├── ContractHash: string
  ├── ContractPackageHash: string
  ├── SchemaURef: URef
  ├── EventsURef: URef
  └── Events: Dictionary<string, CESEventSchema>
        └── CESEventSchema
              ├── EventName: string
              └── Fields: IReadOnlyList<CESEventSchemaField>
                    ├── Name: string
                    └── CLTypeInfo: CLTypeInfo

CESEvent
  ├── Name: string                    (raw name, e.g. "event_Transfer")
  ├── Fields: IReadOnlyList<NamedArg>
  ├── ContractHash: string
  ├── ContractPackageHash: string
  ├── TransformKey: GlobalStateKey
  ├── TransformId: int
  └── EventId: string
```

---

## Real-World Example — Watching Multiple Contracts

The following example loads schemas for two contracts, fetches a transaction, and prints the events emitted:

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Casper.Network.SDK;
using Casper.Network.SDK.CES;

var client = new NetCasperClient("https://node.testnet.casper.network/rpc");

// Load the schema for each contract to watch
var minterSchema = await CESContractSchema.LoadAsync(
    client,
    "hash-1262d06e53125ea098187fb4d1d5b10a7afed48e5e5eef182ed992fc5b100349");

var cep18Schema = await CESContractSchema.LoadAsync(
    client,
    "hash-17cb23ce7b6d663fc82bacbdd56a26dc722cabe7e69c84a3ca729bf3cb7fdc70");

// Fetch a transaction and extract its execution result transforms
var getTxResult = await client.GetTransaction(
    "8e46a16fc8fc15c38405e092959fb20acc44dcfca1d1caecb9bc59d018f50df6");
var txResult = getTxResult.Parse();

// Scan for CES events emitted by either watched contract
var events = CESParser.GetEvents(
    txResult.ExecutionInfo.ExecutionResult.Effect,
    new List<CESContractSchema> { minterSchema, cep18Schema });

// Work with a specific event
var buyEvent = events.FirstOrDefault(e => e.Name == "Buy");
if (buyEvent != null)
{
    Console.WriteLine("Buy token: " + buyEvent["token"].ToGlobalStateKey());
    Console.WriteLine("Buy amount: " + buyEvent["amount_token_out"].ToBigInteger());
}

// Serialize all events to JSON
var json = JsonSerializer.Serialize(events);
Console.WriteLine(json);
```

## Parsing CES events in the SSE stream

See `Docs/Examples/CESParser/Program.cs` for a runnable sample that connects to the node SSE stream, listens to `TransactionProcessed` events, and pretty-prints parsed CES events.

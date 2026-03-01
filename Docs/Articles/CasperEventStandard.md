# Casper Event Standard (CES)

The **Casper Event Standard (CES)** is a convention adopted by make-software for emitting and consuming typed events from Casper smart contracts. Contracts that follow CES store a self-describing schema alongside their events, making it possible to decode any event without out-of-band knowledge of its structure.

The Casper .NET SDK provides `CESParser` in the `Casper.Network.SDK.CES` namespace to handle both schema and event parsing.

---

## How CES Works

A CES-compliant contract uses two special named keys:

| Named key | CLType | Purpose |
|---|---|---|
| `__events_schema` | `Any` | Binary-encoded schema listing every event type and its fields |
| `__events` | `Map(U32, Bytes)` | Ordered map of event index → raw event bytes |

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

All integers are little-endian. The `String` encoding used here is the standard Casper string encoding: a `u32` length followed by the UTF-8 content (no null terminator).

### Events (`__events`)

Each entry in the `__events` map is a `Bytes` (`Vec<u8>`) value. The raw bytes of one event entry are laid out as follows:

```
u32 LE              — Vec<u8> outer length (= remaining bytes; auto-detected and skipped)
String              — event name, prefixed with "event_" (e.g. "event_Mint")
for each field (in schema order):
  raw field bytes   — native Casper serialization, no CLValue wrapper
```

The `"event_"` prefix is stripped internally when looking up the event in the schema, but `CESEvent.Name` preserves the original name exactly as stored in the bytes.

### CLType Binary Encoding

CLType tags used in the schema are single bytes:

| CLType | Tag |
|---|---|
| `Bool` | `0x00` |
| `I32` | `0x01` |
| `I64` | `0x02` |
| `U8` | `0x03` |
| `U32` | `0x04` |
| `U64` | `0x05` |
| `U128` | `0x06` |
| `U256` | `0x07` |
| `U512` | `0x08` |
| `Unit` | `0x09` |
| `String` | `0x0a` |
| `Key` | `0x0b` |
| `URef` | `0x0c` |
| `Option` | `0x0d` + inner type |
| `List` | `0x0e` + item type |
| `ByteArray` | `0x0f` + `u32` size |
| `Result` | `0x10` + ok type + err type |
| `Map` | `0x11` + key type + value type |
| `Tuple1` | `0x12` + type₁ |
| `Tuple2` | `0x13` + type₁ + type₂ |
| `Tuple3` | `0x14` + type₁ + type₂ + type₃ |
| `Any` | `0x15` |
| `PublicKey` | `0x16` |

Compound types (Option, List, Map, etc.) are encoded recursively — the tag is followed immediately by its inner type tags.

---

## API Overview

### Parsing the Schema

Retrieve the `__events_schema` named key from the contract's global state, extract the raw bytes from the CLValue, and pass them to `CESParser.ParseSchema`:

```csharp
using Casper.Network.SDK.CES;

byte[] schemaBytes = /* raw bytes from __events_schema CLValue */;
CESContractSchema schema = CESParser.ParseSchema(schemaBytes);

// Check what events the contract supports
foreach (var kvp in schema.Events)
{
    Console.WriteLine($"Event: {kvp.Key}");
    foreach (var field in kvp.Value.Fields)
        Console.WriteLine($"  {field.Name}: {field.CLTypeInfo.Type}");
}
```

### Parsing an Event

Retrieve the value bytes for one entry from `__events` and pass them together with the schema to `CESParser.ParseEvent`:

```csharp
byte[] eventBytes = /* raw bytes of one entry from __events map */;
CESEvent evt = CESParser.ParseEvent(eventBytes, schema);

Console.WriteLine(evt.Name); // e.g. "event_Mint"

// Access fields by name
NamedArg amount = evt["amount"];
Console.WriteLine(amount.Value.ToBigInteger()); // 1000000000000000000

NamedArg recipient = evt["recipient"];
Console.WriteLine(recipient.Value.ToString()); // hash-1262d0...
```

Each field in `CESEvent.Fields` is a `NamedArg`, exposing the field name and a fully-typed `CLValue` that supports the standard `ToXxx()` conversion methods (`ToBigInteger()`, `ToString()`, `ToBoolean()`, etc.).

### Looking Up an Event Schema

```csharp
if (schema.TryGetEventSchema("Mint", out CESEventSchema mintSchema))
{
    Console.WriteLine($"Mint has {mintSchema.Fields.Count} fields.");
}

// Throws KeyNotFoundException if not found:
CESEventSchema transferSchema = schema.GetEventSchema("Transfer");
```

---

## Data Model

```
CESContractSchema
  └── Events: Dictionary<string, CESEventSchema>
        └── CESEventSchema
              ├── EventName: string
              └── Fields: IReadOnlyList<CESEventSchemaField>
                    ├── Name: string
                    └── CLTypeInfo: CLTypeInfo

CESEvent
  ├── Name: string                    (raw name, e.g. "event_Transfer")
  └── Fields: IReadOnlyList<NamedArg>
        ├── Name: string
        └── Value: CLValue
```

---

## Real-World Example — CEP-18 Token Contract

A CEP-18 fungible token contract exposes seven events: `Burn`, `DecreaseAllowance`, `IncreaseAllowance`, `Mint`, `SetAllowance`, `Transfer`, and `TransferFrom`.

A `Mint` event byte payload (hex) looks like this:

```
38000000                     ← Vec<u8> length = 56 bytes
0a000000                     ← String length = 10
6576656e745f4d696e74         ← "event_Mint"
01                           ← recipient: Key tag (Account/Hash)
1262d06e...349               ← 32-byte key hash
08000000                     ← amount: U512, 8 bytes
64a7b3b6e00d0000             ← 1 000 000 000 000 000 000
```

After parsing:

```csharp
Console.WriteLine(evt.Name);              // event_Mint
Console.WriteLine(evt["recipient"]);      // hash-1262d06e...349
Console.WriteLine(evt["amount"]
    .Value.ToBigInteger());               // 1000000000000000000
```

---

## Notes

- **Schema key** (`__events_schema`) and **events key** (`__events`) are both stored as named keys directly on the contract's `StoredContractByHash` or `StoredContractByName` entity.
- The `Vec<u8>` outer length prefix is auto-detected and skipped transparently by `ParseEvent`.
- `CESEvent.Name` always reflects the exact string found in the event bytes, including the `"event_"` prefix. Schema lookup normalises the name by stripping that prefix automatically.
- This implementation targets the Casper 1.x named-key storage pattern. Casper 2.x may use a different mechanism.

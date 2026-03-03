using System.Numerics;
using System.Text.Json;
using Casper.Network.SDK.CES;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    /// <summary>
    /// Verifies that <see cref="CESContractSchema"/> and <see cref="CESEvent"/> round-trip
    /// correctly through System.Text.Json serialization/deserialization using the
    /// [JsonPropertyName] and [JsonConstructor] annotations added to those classes.
    /// </summary>
    public class CESJsonSerializerTest
    {
        // ── shared fixtures (same as CESParserTest) ───────────────────────────
        //
        // Schema: one event "Transfer" with fields amount (U512) and sender (String)
        private static readonly string SchemaHex = string.Concat(
            "01000000",
            "08000000",
            "5472616e73666572",
            "02000000",
            "06000000", "616d6f756e74", "08",
            "06000000", "73656e646572", "0a"
        );

        // event_Transfer(amount=100, sender="Alice")
        private static readonly string EventHex = string.Concat(
            "0e000000",
            "6576656e745f5472616e73666572",
            "01", "64",
            "05000000", "416c696365"
        );

        private const string ContractHash = "hash-aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd";
        private const string ContractPkgHash = "contract-package-hash-1122334411223344112233441122334411223344112233441122334411223344";

        // ── helpers ───────────────────────────────────────────────────────────

        private static CESContractSchema BuildAnnotatedSchema() =>
            new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(SchemaHex)).Events)
            {
                ContractHash = ContractHash,
                ContractPackageHash = ContractPkgHash,
            };

        private static CESEvent BuildAnnotatedEvent()
        {
            var schema = BuildAnnotatedSchema();
            return CESEvent.ParseEvent(Hex.Decode(EventHex), schema, transformId: 3, eventId: "42");
        }

        // ── CESEventSchemaField ───────────────────────────────────────────────

        [Test]
        public void SchemaField_RoundTrip_PreservesNameAndCLType()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var field = schema.Events["Transfer"].Fields[0];

            var json = JsonSerializer.Serialize(field);
            var restored = JsonSerializer.Deserialize<CESEventSchemaField>(json);

            Assert.AreEqual(field.Name, restored.Name);
            Assert.AreEqual(field.CLTypeInfo.Type, restored.CLTypeInfo.Type);
        }

        [Test]
        public void SchemaField_Serializes_SnakeCasePropertyNames()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var field = schema.Events["Transfer"].Fields[0];

            var json = JsonSerializer.Serialize(field);

            Assert.IsTrue(json.Contains("\"name\""));
            Assert.IsTrue(json.Contains("\"cl_type\""));
        }

        // ── CESEventSchema ────────────────────────────────────────────────────

        [Test]
        public void EventSchema_RoundTrip_PreservesEventNameAndFields()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var eventSchema = schema.Events["Transfer"];

            var json = JsonSerializer.Serialize(eventSchema);
            var restored = JsonSerializer.Deserialize<CESEventSchema>(json);

            Assert.AreEqual("Transfer", restored.EventName);
            Assert.AreEqual(2, restored.Fields.Count);
            Assert.AreEqual("amount", restored.Fields[0].Name);
            Assert.AreEqual(CLType.U512, restored.Fields[0].CLTypeInfo.Type);
            Assert.AreEqual("sender", restored.Fields[1].Name);
            Assert.AreEqual(CLType.String, restored.Fields[1].CLTypeInfo.Type);
        }

        [Test]
        public void EventSchema_Serializes_SnakeCasePropertyNames()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var eventSchema = schema.Events["Transfer"];

            var json = JsonSerializer.Serialize(eventSchema);

            Assert.IsTrue(json.Contains("\"event_name\""));
            Assert.IsTrue(json.Contains("\"fields\""));
        }

        // ── CESContractSchema ─────────────────────────────────────────────────

        [Test]
        public void ContractSchema_RoundTrip_PreservesEvents()
        {
            var schema = BuildAnnotatedSchema();

            var json = JsonSerializer.Serialize(schema);
            var restored = JsonSerializer.Deserialize<CESContractSchema>(json);

            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.TryGetEventSchema("Transfer", out var evt));
            Assert.AreEqual("Transfer", evt.EventName);
            Assert.AreEqual(2, evt.Fields.Count);
        }

        [Test]
        public void ContractSchema_RoundTrip_PreservesContractHash()
        {
            var schema = BuildAnnotatedSchema();

            var json = JsonSerializer.Serialize(schema);
            var restored = JsonSerializer.Deserialize<CESContractSchema>(json);

            Assert.AreEqual(ContractHash, restored.ContractHash);
        }

        [Test]
        public void ContractSchema_RoundTrip_PreservesContractPackageHash()
        {
            var schema = BuildAnnotatedSchema();

            var json = JsonSerializer.Serialize(schema);
            var restored = JsonSerializer.Deserialize<CESContractSchema>(json);

            Assert.AreEqual(ContractPkgHash, restored.ContractPackageHash);
        }

        [Test]
        public void ContractSchema_Serializes_SnakeCasePropertyNames()
        {
            var schema = BuildAnnotatedSchema();

            var json = JsonSerializer.Serialize(schema);

            Assert.IsTrue(json.Contains("\"events\""));
            Assert.IsTrue(json.Contains("\"contract_hash\""));
            Assert.IsTrue(json.Contains("\"contract_package_hash\""));
        }

        [Test]
        public void ContractSchema_WithNullHashes_RoundTripPreservesNulls()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            var json = JsonSerializer.Serialize(schema);
            var restored = JsonSerializer.Deserialize<CESContractSchema>(json);

            Assert.IsNull(restored.ContractHash);
            Assert.IsNull(restored.ContractPackageHash);
        }

        // ── CESEvent ──────────────────────────────────────────────────────────

        [Test]
        public void CESEvent_RoundTrip_PreservesName()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);
            var restored = JsonSerializer.Deserialize<CESEvent>(json);

            Assert.AreEqual("event_Transfer", restored.Name);
        }

        [Test]
        public void CESEvent_RoundTrip_PreservesFieldCount()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);
            var restored = JsonSerializer.Deserialize<CESEvent>(json);

            Assert.AreEqual(2, restored.Fields.Count);
        }

        [Test]
        public void CESEvent_RoundTrip_PreservesU512Field()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);
            var restored = JsonSerializer.Deserialize<CESEvent>(json);

            var amount = restored["amount"];
            Assert.IsNotNull(amount);
            Assert.AreEqual(CLType.U512, amount.Value.TypeInfo.Type);
            Assert.AreEqual(new BigInteger(100), amount.Value.ToBigInteger());
        }

        [Test]
        public void CESEvent_RoundTrip_PreservesStringField()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);
            var restored = JsonSerializer.Deserialize<CESEvent>(json);

            var sender = restored["sender"];
            Assert.IsNotNull(sender);
            Assert.AreEqual(CLType.String, sender.Value.TypeInfo.Type);
            Assert.AreEqual("Alice", sender.Value.ToString());
        }

        [Test]
        public void CESEvent_RoundTrip_PreservesContextFields()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);
            var restored = JsonSerializer.Deserialize<CESEvent>(json);

            Assert.AreEqual(ContractHash, restored.ContractHash);
            Assert.AreEqual(ContractPkgHash, restored.ContractPackageHash);
            Assert.AreEqual(3, restored.TransformId);
            Assert.AreEqual("42", restored.EventId);
        }

        [Test]
        public void CESEvent_Serializes_SnakeCasePropertyNames()
        {
            var evt = BuildAnnotatedEvent();

            var json = JsonSerializer.Serialize(evt);

            Assert.IsTrue(json.Contains("\"name\""));
            Assert.IsTrue(json.Contains("\"fields\""));
            Assert.IsTrue(json.Contains("\"contract_hash\""));
            Assert.IsTrue(json.Contains("\"contract_package_hash\""));
            Assert.IsTrue(json.Contains("\"transform_id\""));
            Assert.IsTrue(json.Contains("\"event_id\""));
        }

        [Test]
        public void CESEvent_FieldsSerializeAsArrayOfPairs()
        {
            var evt = BuildAnnotatedEvent();

            // Each field must serialize as ["fieldName", {CLValue}], not as an object.
            var json = JsonSerializer.Serialize(evt);
            using var doc = JsonDocument.Parse(json);
            var fields = doc.RootElement.GetProperty("fields");

            Assert.AreEqual(JsonValueKind.Array, fields.ValueKind);

            var first = fields[0];
            Assert.AreEqual(JsonValueKind.Array, first.ValueKind);
            Assert.AreEqual("amount", first[0].GetString());
            Assert.AreEqual(JsonValueKind.Object, first[1].ValueKind);

            var second = fields[1];
            Assert.AreEqual("sender", second[0].GetString());
        }
    }
}

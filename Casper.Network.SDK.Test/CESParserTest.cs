using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime;
using Casper.Network.SDK.CES;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    [TestFixture]
    public class CESParserTest
    {
        // Schema binary for one event "Transfer" with two fields:
        //   amount (CLType.U512 = 0x08)
        //   sender (CLType.String = 0x0a)
        private static readonly string SchemaHex = string.Concat(
            "01000000",         // 1 event
            "08000000",         // len("Transfer") = 8
            "5472616e73666572", // "Transfer"
            "02000000",         // 2 fields
            "06000000",         // len("amount") = 6
            "616d6f756e74",     // "amount"
            "08",               // CLType.U512
            "06000000",         // len("sender") = 6
            "73656e646572",     // "sender"
            "0a"                // CLType.String
        );

        // Event binary for event_Transfer(amount=100, sender="Alice")
        // U512(100) serializes as: 0x01 (length byte) + 0x64 (100 in one byte)
        // String("Alice") serializes as: 0x05000000 (length) + 0x416c696365
        private static readonly string EventHex = string.Concat(
            "0e000000",                 // len("event_Transfer") = 14
            "6576656e745f5472616e73666572", // "event_Transfer"
            "01",                       // U512 length = 1 byte
            "64",                       // U512 value = 100 (0x64)
            "05000000",                 // len("Alice") = 5
            "416c696365"                // "Alice"
        );

        [Test]
        public void ParseSchema_SingleEvent_CorrectEventName()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            Assert.IsTrue(schema.TryGetEventSchema("Transfer", out var eventSchema));
            Assert.AreEqual("Transfer", eventSchema.EventName);
        }

        [Test]
        public void ParseSchema_SingleEvent_CorrectFieldCount()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            schema.TryGetEventSchema("Transfer", out var eventSchema);
            Assert.AreEqual(2, eventSchema.Fields.Count);
        }

        [Test]
        public void ParseSchema_SingleEvent_CorrectFieldDefinitions()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            schema.TryGetEventSchema("Transfer", out var eventSchema);

            Assert.AreEqual("amount", eventSchema.Fields[0].Name);
            Assert.AreEqual(CLType.U512, eventSchema.Fields[0].CLTypeInfo.Type);

            Assert.AreEqual("sender", eventSchema.Fields[1].Name);
            Assert.AreEqual(CLType.String, eventSchema.Fields[1].CLTypeInfo.Type);
        }

        [Test]
        public void ParseSchema_UnknownEvent_ReturnsFalse()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            Assert.IsFalse(schema.TryGetEventSchema("Mint", out _));
        }

        [Test]
        public void ParseEvent_CorrectEventName()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            Assert.AreEqual("event_Transfer", evt.Name);
        }

        [Test]
        public void ParseEvent_CorrectFieldCount()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            Assert.AreEqual(2, evt.Fields.Count);
        }

        [Test]
        public void ParseEvent_U512FieldCorrectValue()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            var amountField = evt["amount"];
            Assert.IsNotNull(amountField);
            Assert.AreEqual(CLType.U512, amountField.Value.TypeInfo.Type);
            Assert.AreEqual(new BigInteger(100), amountField.Value.ToBigInteger());
        }

        [Test]
        public void ParseEvent_StringFieldCorrectValue()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            var senderField = evt["sender"];
            Assert.IsNotNull(senderField);
            Assert.AreEqual(CLType.String, senderField.Value.TypeInfo.Type);
            Assert.AreEqual("Alice", senderField.Value.ToString());
        }

        [Test]
        public void ParseEvent_IndexerReturnsNullForMissingField()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            Assert.IsNull(evt["nonexistent"]);
        }

        [Test]
        public void ParseEvent_UnknownEventName_ThrowsKeyNotFoundException()
        {
            var schema = CESContractSchema.ParseSchema(Hex.Decode(SchemaHex));

            // Build event bytes with an event name ("event_Mint") not present in the schema.
            var mintEventHex = string.Concat(
                "0a000000",             // len("event_Mint") = 10
                "6576656e745f4d696e74"  // "event_Mint"
            );

            Assert.Throws<KeyNotFoundException>(
                () => CESEvent.ParseEvent(Hex.Decode(mintEventHex), schema));
        }

        [Test]
        public void ParseSchema_OptionType_CorrectInnerType()
        {
            // Schema with one event "Approve" and one Option(PublicKey) field "operator"
            // CLType.Option = 0x0d, CLType.PublicKey = 0x16
            var schemaHex = string.Concat(
                "01000000",         // 1 event
                "07000000",         // len("Approve") = 7
                "417070726f7665",   // "Approve"
                "01000000",         // 1 field
                "08000000",         // len("operator") = 8
                "6f70657261746f72", // "operator"
                "0d",               // CLType.Option
                "16"                // CLType.PublicKey
            );

            var schema = CESContractSchema.ParseSchema(Hex.Decode(schemaHex));

            schema.TryGetEventSchema("Approve", out var eventSchema);
            Assert.AreEqual("operator", eventSchema.Fields[0].Name);
            Assert.AreEqual(CLType.Option, eventSchema.Fields[0].CLTypeInfo.Type);

            var optionType = (CLOptionTypeInfo)eventSchema.Fields[0].CLTypeInfo;
            Assert.AreEqual(CLType.PublicKey, optionType.OptionType.Type);
        }

        [Test]
        public void ParseSchema_MapType_CorrectKeyAndValueTypes()
        {
            // Schema with one event "Allowances" and one Map(PublicKey, U256) field "data"
            // CLType.Map = 0x11, CLType.PublicKey = 0x16, CLType.U256 = 0x07
            var schemaHex = string.Concat(
                "01000000",         // 1 event
                "0a000000",         // len("Allowances") = 10
                "416c6c6f77616e636573", // "Allowances"
                "01000000",         // 1 field
                "04000000",         // len("data") = 4
                "64617461",         // "data"
                "11",               // CLType.Map
                "16",               // key: CLType.PublicKey
                "07"                // value: CLType.U256
            );

            var schema = CESContractSchema.ParseSchema(Hex.Decode(schemaHex));

            schema.TryGetEventSchema("Allowances", out var eventSchema);
            Assert.AreEqual("data", eventSchema.Fields[0].Name);
            Assert.AreEqual(CLType.Map, eventSchema.Fields[0].CLTypeInfo.Type);

            var mapType = (CLMapTypeInfo)eventSchema.Fields[0].CLTypeInfo;
            Assert.AreEqual(CLType.PublicKey, mapType.KeyType.Type);
            Assert.AreEqual(CLType.U256, mapType.ValueType.Type);
        }
        
        [Test]
        public void ParseSchema_TupleType_CorrectInnerTypes()
        {
            // Schema of a CEP-18 contract
            var schemaHex =
                "07000000040000004275726e02000000050000006f776e65720b06000000616d6f756e74071100000044656372656173" + 
                "65416c6c6f77616e636504000000050000006f776e65720b070000007370656e6465720b09000000616c6c6f77616e63"  +
                "650707000000646563725f62790711000000496e637265617365416c6c6f77616e636504000000050000006f776e6572"+
                "0b070000007370656e6465720b09000000616c6c6f77616e63650706000000696e635f627907040000004d696e740200" +
                "000009000000726563697069656e740b06000000616d6f756e74070c000000536574416c6c6f77616e63650300000005" +
                "0000006f776e65720b070000007370656e6465720b09000000616c6c6f77616e636507080000005472616e7366657203" +
                "0000000600000073656e6465720b09000000726563697069656e740b06000000616d6f756e74070c0000005472616e73" +
                "66657246726f6d04000000070000007370656e6465720b050000006f776e65720b09000000726563697069656e740b06" +
                "000000616d6f756e7407";
            
            var schema = CESContractSchema.ParseSchema(Hex.Decode(schemaHex));
            Assert.IsNotNull(schema);
            schema.TryGetEventSchema("Transfer", out var eventSchema);
            Assert.IsNotNull(eventSchema);
            Assert.AreEqual("Transfer", eventSchema.EventName);

            var evt0 =
                "0a0000006576656e745f4d696e74011262d06e53125ea098187fb4d1d5b10a7afed48e5e5eef182ed992fc5b10034908000064a7b3b6e00d";
            var parsedEvt = CESEvent.ParseEvent(Hex.Decode(evt0), schema);
            Assert.IsNotNull(parsedEvt);
            Assert.AreEqual("event_Mint", parsedEvt.Name);
            // var amount = parsedEvt["amount"];
            // amount.Value
        }
    }
}

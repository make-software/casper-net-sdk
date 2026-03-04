using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.CES;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    [TestFixture]
    public class CESParserGetEventsTest
    {
        // ─── shared schema / event fixtures ────────────────────────────────────
        //
        // Schema: one event "Transfer" with fields amount (U512) and sender (String)

        private static readonly string SchemaHex = string.Concat(
            "01000000",              // 1 event
            "08000000",              // len("Transfer") = 8
            "5472616e73666572",      // "Transfer"
            "02000000",              // 2 fields
            "06000000",              // len("amount") = 6
            "616d6f756e74",          // "amount"
            "08",                    // CLType.U512
            "06000000",              // len("sender") = 6
            "73656e646572",          // "sender"
            "0a"                     // CLType.String
        );

        // event_Transfer(amount=100, sender="Alice")
        //   U512(100)   → length byte 0x01 + value 0x64
        //   String      → 4-byte LE length + UTF-8
        private static readonly string EventHex = string.Concat(
            "0e000000",                      // CLString len = 14
            "6576656e745f5472616e73666572",  // "event_Transfer"
            "01", "64",                      // U512(100)
            "05000000", "416c696365"         // String("Alice")
        );

        // event_Transfer(amount=200, sender="Bob")
        private static readonly string EventBobHex = string.Concat(
            "0e000000",                      // CLString len = 14
            "6576656e745f5472616e73666572",  // "event_Transfer"
            "01", "c8",                      // U512(200)
            "03000000", "426f62"             // String("Bob")
        );

        // Schema for a second contract: one event "Mint" with field recipient (U512)
        private static readonly string SchemaHex2 = string.Concat(
            "01000000",                  // 1 event
            "04000000",                  // len("Mint") = 4
            "4d696e74",                  // "Mint"
            "01000000",                  // 1 field
            "09000000",                  // len("recipient") = 9
            "726563697069656e74",        // "recipient"
            "08"                         // CLType.U512
        );

        // event_Mint(recipient=200)
        private static readonly string EventMintHex = string.Concat(
            "0a000000",                  // CLString len = 10
            "6576656e745f4d696e74",      // "event_Mint"
            "01", "c8"                   // U512(200)
        );

        // 32-byte seeds (hex-encoded, 64 chars each)
        private const string SeedHex =
            "aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd";

        private const string SeedHex2 =
            "1122334411223344112233441122334411223344112233441122334411223344";

        // 32-byte dictionary global-state key hashes (different from the seeds)
        private const string DictKeyHex =
            "deadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef";

        private const string DictKeyHex2 =
            "cafebabecafebabecafebabecafebabecafebabecafebabecafebabecafebabe";

        // ─── helpers ───────────────────────────────────────────────────────────

        private static CESContractSchema ParseSchema(string schemaHex) =>
            CESContractSchema.ParseSchema(Hex.Decode(schemaHex));

        private static CESContractSchema MakeWatchedSchema(string schemaHex, string seedHex) =>
            new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(schemaHex)).Events)
            {
                EventsURef = new URef($"uref-{seedHex}-000")
            };

        /// <summary>
        /// Builds the raw bytes that <see cref="CLValueDictionary.Parse"/> expects.
        /// Format:
        ///   [4B outer length] [CLValue blob (length + data + type)] [4B seed len] [seed bytes]
        ///   [4B key len] [key bytes]
        /// The event payload is wrapped as a CLValue.ByteArray so that
        /// <c>dict.Value</c> equals the original <paramref name="eventPayload"/> bytes.
        /// </summary>
        private static byte[] BuildDictBytes(byte[] eventPayload, byte[] seedBytes, string itemKey)
        {
            // CLValueByteSerializer.ToBytes produces the exact bytes that FromReader consumes:
            //   [4B data length] [N data bytes] [type bytes]
            var clValueBlob = new CLValueByteSerializer().ToBytes(CLValue.ByteArray(eventPayload));

            using var ms = new MemoryStream();

            // outer length = total bytes that FromReader() will read
            WriteU32LE(ms, (uint)clValueBlob.Length);
            ms.Write(clValueBlob, 0, clValueBlob.Length);

            // seed: 4-byte LE count + raw bytes
            WriteU32LE(ms, (uint)seedBytes.Length);
            ms.Write(seedBytes, 0, seedBytes.Length);

            // item key: 4-byte LE count + UTF-8
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(itemKey);
            WriteU32LE(ms, (uint)keyBytes.Length);
            ms.Write(keyBytes, 0, keyBytes.Length);

            return ms.ToArray();
        }

        private static void WriteU32LE(MemoryStream ms, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            ms.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Creates a <see cref="Transform"/> that looks exactly like a CES dictionary write.
        /// </summary>
        private static Transform MakeCESTransform(
            string dictKeyHex, byte[] eventPayload, byte[] seedBytes, string itemKey = "1")
        {
            var clValue = new CLValue(
                BuildDictBytes(eventPayload, seedBytes, itemKey),
                new CLTypeInfo(CLType.Any));

            return new Transform
            {
                Key = GlobalStateKey.FromString($"dictionary-{dictKeyHex}"),
                Kind = new WriteTransformKind { Value = new StoredValue { CLValue = clValue } }
            };
        }

        // ─── guard tests ───────────────────────────────────────────────────────

        [Test]
        public void GetEvents_NullTransforms_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CESParser.GetEvents(null, new List<CESContractSchema>()));
        }

        [Test]
        public void GetEvents_NullWatchedContracts_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CESParser.GetEvents(new List<Transform>(), null));
        }

        // ─── empty inputs ──────────────────────────────────────────────────────

        [Test]
        public void GetEvents_EmptyTransforms_ReturnsEmptyList()
        {
            var result = CESParser.GetEvents(
                new List<Transform>(),
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_EmptyWatchedContracts_ReturnsEmptyList()
        {
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema>());

            Assert.AreEqual(0, result.Count);
        }

        // ─── filter: key type ──────────────────────────────────────────────────

        [Test]
        public void GetEvents_AccountHashKeyTransform_IsSkipped()
        {
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString(
                    "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"),
                Kind = new WriteTransformKind
                {
                    Value = new StoredValue
                    {
                        CLValue = new CLValue(
                            BuildDictBytes(Hex.Decode(EventHex), Hex.Decode(SeedHex), "1"),
                            new CLTypeInfo(CLType.Any))
                    }
                }
            };

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_HashKeyTransform_IsSkipped()
        {
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString(
                    "hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"),
                Kind = new WriteTransformKind
                {
                    Value = new StoredValue
                    {
                        CLValue = new CLValue(
                            BuildDictBytes(Hex.Decode(EventHex), Hex.Decode(SeedHex), "1"),
                            new CLTypeInfo(CLType.Any))
                    }
                }
            };

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        // ─── filter: transform kind ────────────────────────────────────────────

        [Test]
        public void GetEvents_IdentityTransformOnDictionaryKey_IsSkipped()
        {
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString($"dictionary-{DictKeyHex}"),
                Kind = new IdentityTransformKind()
            };

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_AddInt32TransformOnDictionaryKey_IsSkipped()
        {
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString($"dictionary-{DictKeyHex}"),
                Kind = new AddInt32TransformKind { Value = 42 }
            };

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_WriteWithoutCLValue_IsSkipped()
        {
            // Write of an Account (or any other StoredValue without CLValue set)
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString($"dictionary-{DictKeyHex}"),
                Kind = new WriteTransformKind { Value = new StoredValue() }
            };

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        // ─── filter: seed matching ─────────────────────────────────────────────

        [Test]
        public void GetEvents_DictionaryWriteWithUnwatchedSeed_IsSkipped()
        {
            const string differentSeed =
                "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";

            var transform = MakeCESTransform(
                DictKeyHex, Hex.Decode(EventHex), Hex.Decode(differentSeed));

            var result = CESParser.GetEvents(
                new List<Transform> { transform },
                new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) });

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_SeedMatchIgnoresAccessRights()
        {
            // CLValueDictionary.Parse always creates the seed with AccessRights.NONE.
            // The watched-contract key is the plain 32-byte hash (no access rights).
            // GetEvents must still find the match.
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(1, result.Count);
        }

        // ─── happy-path: single event ──────────────────────────────────────────

        [Test]
        public void GetEvents_SingleMatchingTransform_ReturnsSingleEvent()
        {
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetEvents_SingleMatchingTransform_CorrectEventName()
        {
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual("Transfer", result[0].Name);
        }

        [Test]
        public void GetEvents_SingleMatchingTransform_CorrectU512Field()
        {
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(new BigInteger(100), result[0]["amount"].ToBigInteger());
        }

        [Test]
        public void GetEvents_SingleMatchingTransform_CorrectStringField()
        {
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual("Alice", result[0]["sender"].ToString());
        }

        // ─── graceful skipping ─────────────────────────────────────────────────

        [Test]
        public void GetEvents_EventNameAbsentFromSchema_IsSkipped()
        {
            // EventMintHex has name "event_Mint", but the watched schema only knows "Transfer".
            // ParseEvent throws KeyNotFoundException → GetEvents silently skips.
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventMintHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetEvents_UnparsableDictionaryBytes_IsSkipped()
        {
            // Garbage bytes that CLValueDictionary.Parse will fail on → skipped, no exception.
            var clValue = new CLValue(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, new CLTypeInfo(CLType.Any));
            var transform = new Transform
            {
                Key = GlobalStateKey.FromString($"dictionary-{DictKeyHex}"),
                Kind = new WriteTransformKind { Value = new StoredValue { CLValue = clValue } }
            };
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            Assert.DoesNotThrow(() =>
            {
                var result = CESParser.GetEvents(new List<Transform> { transform }, watched);
                Assert.AreEqual(0, result.Count);
            });
        }

        // ─── multiple events ───────────────────────────────────────────────────

        [Test]
        public void GetEvents_TwoEventsFromSameContract_ReturnsBoth()
        {
            // Two dictionary writes with the same seed but different item keys
            var t1 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),    Hex.Decode(SeedHex), "1");
            var t2 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventBobHex), Hex.Decode(SeedHex), "2");
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetEvents_TwoEventsFromDifferentContracts_ReturnsBoth()
        {
            var t1 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),     Hex.Decode(SeedHex));
            var t2 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventMintHex), Hex.Decode(SeedHex2));
            var watched = new List<CESContractSchema>
            {
                MakeWatchedSchema(SchemaHex,  SeedHex),
                MakeWatchedSchema(SchemaHex2, SeedHex2),
            };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetEvents_TwoEventsFromDifferentContracts_CorrectFieldValues()
        {
            var t1 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),     Hex.Decode(SeedHex));
            var t2 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventMintHex), Hex.Decode(SeedHex2));
            var watched = new List<CESContractSchema>
            {
                MakeWatchedSchema(SchemaHex,  SeedHex),
                MakeWatchedSchema(SchemaHex2, SeedHex2),
            };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            // first event: Transfer – check both fields
            Assert.AreEqual("Transfer", result[0].Name);
            Assert.AreEqual(new BigInteger(100), result[0]["amount"].ToBigInteger());
            Assert.AreEqual("Alice", result[0]["sender"].ToString());

            // second event: Mint – check recipient
            Assert.AreEqual("Mint", result[1].Name);
            Assert.AreEqual(new BigInteger(200), result[1]["recipient"].ToBigInteger());
        }

        // ─── ordering and mixed transforms ────────────────────────────────────

        [Test]
        public void GetEvents_OrderPreserved_MatchesTransformOrder()
        {
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            // Alice (item "1") comes before Bob (item "2") in the transform list
            var t1 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),    Hex.Decode(SeedHex), "1");
            var t2 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventBobHex), Hex.Decode(SeedHex), "2");

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Alice", result[0]["sender"].ToString());
            Assert.AreEqual("Bob",   result[1]["sender"].ToString());
        }

        [Test]
        public void GetEvents_MixedTransforms_OnlyMatchingEventsReturned()
        {
            // t1: Identity on a dictionary key → skipped (wrong kind)
            var t1 = new Transform
            {
                Key  = GlobalStateKey.FromString($"dictionary-{DictKeyHex}"),
                Kind = new IdentityTransformKind()
            };
            // t2: CES event from watched contract → included
            var t2 = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            // t3: Write of CLValue to an account-hash key → skipped (wrong key type)
            var t3 = new Transform
            {
                Key  = GlobalStateKey.FromString(
                    "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"),
                Kind = new WriteTransformKind { Value = new StoredValue { CLValue = CLValue.U32(1) } }
            };
            // t4: dictionary write with an unwatched seed → skipped
            var t4 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventHex), Hex.Decode(SeedHex2));

            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2, t3, t4 }, watched);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Transfer", result[0].Name);
        }

        [Test]
        public void GetEvents_OneValidOneBadParseable_OnlyValidEventReturned()
        {
            // t1: valid CES event
            var t1 = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            // t2: garbled CLValue bytes that will fail CLValueDictionary.Parse → skipped
            var t2 = new Transform
            {
                Key  = GlobalStateKey.FromString($"dictionary-{DictKeyHex2}"),
                Kind = new WriteTransformKind
                {
                    Value = new StoredValue
                    {
                        CLValue = new CLValue(new byte[] { 0x00, 0x01 }, new CLTypeInfo(CLType.Any))
                    }
                }
            };
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Transfer", result[0].Name);
        }

        // ─── TransformId and EventId ───────────────────────────────────────────

        [Test]
        public void GetEvents_SingleEvent_TransformIdIsIndexInList()
        {
            // t0 is a non-matching transform at index 0; t1 is the CES event at index 1.
            var t0 = new Transform
            {
                Key  = GlobalStateKey.FromString(
                    "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7"),
                Kind = new IdentityTransformKind()
            };
            var t1 = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex), "7");
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t0, t1 }, watched);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].TransformId); // index 1 in the list
        }

        [Test]
        public void GetEvents_SingleEvent_EventIdMatchesDictItemKey()
        {
            var t0 = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex), itemKey: "42");
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t0 }, watched);

            Assert.AreEqual("42", result[0].EventId);
        }

        [Test]
        public void GetEvents_MultipleEvents_TransformIdsAreDistinct()
        {
            var t0 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),    Hex.Decode(SeedHex), "1");
            var t1 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventBobHex), Hex.Decode(SeedHex), "2");
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t0, t1 }, watched);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(0, result[0].TransformId);
            Assert.AreEqual(1, result[1].TransformId);
        }

        [Test]
        public void GetEvents_MultipleEvents_EventIdsMatchItemKeys()
        {
            var t0 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),    Hex.Decode(SeedHex), "100");
            var t1 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventBobHex), Hex.Decode(SeedHex), "101");
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };

            var result = CESParser.GetEvents(new List<Transform> { t0, t1 }, watched);

            Assert.AreEqual("100", result[0].EventId);
            Assert.AreEqual("101", result[1].EventId);
        }

        // ─── ContractHash / ContractPackageHash propagation ───────────────────

        [Test]
        public void GetEvents_SchemaWithContractHash_EventCarriesContractHash()
        {
            const string contractHash = "hash-aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd";
            var schema = new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(SchemaHex)).Events)
            {
                ContractHash = contractHash,
                EventsURef = new URef($"uref-{SeedHex}-000"),
            };
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { schema };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(contractHash, result[0].ContractHash);
        }

        [Test]
        public void GetEvents_SchemaWithContractPackageHash_EventCarriesContractPackageHash()
        {
            const string pkgHash = "contract-package-hash-1122334411223344112233441122334411223344112233441122334411223344";
            var schema = new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(SchemaHex)).Events)
            {
                ContractPackageHash = pkgHash,
                EventsURef = new URef($"uref-{SeedHex}-000"),
            };
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));
            var watched = new List<CESContractSchema> { schema };

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.AreEqual(pkgHash, result[0].ContractPackageHash);
        }

        [Test]
        public void GetEvents_SchemaWithoutHashes_EventHashesAreNull()
        {
            var watched = new List<CESContractSchema> { MakeWatchedSchema(SchemaHex, SeedHex) };
            var transform = MakeCESTransform(DictKeyHex, Hex.Decode(EventHex), Hex.Decode(SeedHex));

            var result = CESParser.GetEvents(new List<Transform> { transform }, watched);

            Assert.IsNull(result[0].ContractHash);
            Assert.IsNull(result[0].ContractPackageHash);
        }

        [Test]
        public void GetEvents_TwoDifferentContracts_EventsCarryCorrectHashes()
        {
            const string hash1 = "hash-aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd";
            const string hash2 = "hash-1122334411223344112233441122334411223344112233441122334411223344";

            var schema1 = new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(SchemaHex)).Events)
                { ContractHash = hash1, EventsURef = new URef($"uref-{SeedHex}-000") };
            var schema2 = new CESContractSchema(CESContractSchema.ParseSchema(Hex.Decode(SchemaHex2)).Events)
                { ContractHash = hash2, EventsURef = new URef($"uref-{SeedHex2}-000") };

            var t1 = MakeCESTransform(DictKeyHex,  Hex.Decode(EventHex),     Hex.Decode(SeedHex));
            var t2 = MakeCESTransform(DictKeyHex2, Hex.Decode(EventMintHex), Hex.Decode(SeedHex2));
            var watched = new List<CESContractSchema> { schema1, schema2 };

            var result = CESParser.GetEvents(new List<Transform> { t1, t2 }, watched);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(hash1, result[0].ContractHash);
            Assert.AreEqual(hash2, result[1].ContractHash);
        }

        // ─── ParseEvent in isolation ───────────────────────────────────────────

        [Test]
        public void ParseEvent_InIsolation_TransformIdIsZeroAndEventIdIsNull()
        {
            var schema = ParseSchema(SchemaHex);
            var evt = CESEvent.ParseEvent(Hex.Decode(EventHex), schema);

            Assert.AreEqual(0, evt.TransformId);
            Assert.IsNull(evt.TransformKey);
            Assert.IsNull(evt.EventId);
        }
    }
}

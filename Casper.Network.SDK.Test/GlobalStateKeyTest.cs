using System;
using System.Linq;
using System.Text.Json;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class GlobalStateKeyTest
    {
        [Test]
        public void AccountHashTest()
        {
            const string accountHash = "account-hash-0101010101010101010101010101010101010101010101010101010101010101";
            var key = GlobalStateKey.FromString(accountHash);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is AccountHashKey);
            Assert.AreEqual(accountHash, key.ToString());

            var bytes = Hex.Decode("000101010101010101010101010101010101010101010101010101010101010101");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is AccountHashKey);
            Assert.AreEqual(accountHash, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void AccountHashFromPublicKeyTest()
        {
            const string ed25519Key = "01b7c7c545dfa3fb853a97fb3581ce10eb4f67a5861abed6e70e5e3312fdde402c";
            var publicKey = PublicKey.FromHexString(ed25519Key);

            var key2 = new AccountHashKey(publicKey);
            Assert.IsNotNull(key2);
            Assert.IsTrue(key2.ToString().StartsWith("account-hash-"));
        }

        [Test]
        public void HashKeyTest()
        {
            const string hashKey = "hash-0202020202020202020202020202020202020202020202020202020202020202";
            var key = GlobalStateKey.FromString(hashKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is HashKey);
            Assert.AreEqual(hashKey, key.ToString());

            var bytes = Hex.Decode("010202020202020202020202020202020202020202020202020202020202020202");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is HashKey);
            Assert.AreEqual(hashKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void URefTest()
        {
            const string urefKey = "uref-0303030303030303030303030303030303030303030303030303030303030303-001";
            var key = GlobalStateKey.FromString(urefKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is URef);
            Assert.AreEqual(urefKey, key.ToString());       
            Assert.AreEqual(AccessRights.READ, ((URef)key).AccessRights);

            var bytes = Hex.Decode("02030303030303030303030303030303030303030303030303030303030303030301");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is URef);
            Assert.AreEqual(urefKey, key.ToString());       
            Assert.AreEqual(AccessRights.READ, ((URef)key).AccessRights);
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void TransferKeyTest()
        {
            const string transferKey = "transfer-0404040404040404040404040404040404040404040404040404040404040404";
            var key = GlobalStateKey.FromString(transferKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is TransferKey);
            Assert.AreEqual(transferKey, key.ToString());

            var bytes = Hex.Decode("030404040404040404040404040404040404040404040404040404040404040404");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is TransferKey);
            Assert.AreEqual(transferKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void DeployInfoKeyTest()
        {
            const string deployKey = "deploy-0505050505050505050505050505050505050505050505050505050505050505";
            var key = GlobalStateKey.FromString(deployKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DeployInfoKey);
            Assert.AreEqual(deployKey, key.ToString());

            var bytes = Hex.Decode("040505050505050505050505050505050505050505050505050505050505050505");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DeployInfoKey);
            Assert.AreEqual(deployKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void EraInfoKeyTest()
        {
            const string eraInfoKey = "era-12345";
            var key = GlobalStateKey.FromString(eraInfoKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is EraInfoKey);
            Assert.AreEqual(eraInfoKey, key.ToString());

            var bytes = Hex.Decode("053930000000000000");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is EraInfoKey);
            Assert.AreEqual(eraInfoKey, key.ToString());
            
            var ex = Assert.Catch(() => GlobalStateKey.FromString("era-AAAAA"));
            Assert.IsNotNull(ex);
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void BalanceKeyTest()
        {
            const string balanceKey = "balance-0707070707070707070707070707070707070707070707070707070707070707";
            var key = GlobalStateKey.FromString(balanceKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BalanceKey);
            Assert.AreEqual(balanceKey, key.ToString());

            var bytes = Hex.Decode("060707070707070707070707070707070707070707070707070707070707070707");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BalanceKey);
            Assert.AreEqual(balanceKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void BidKeyTest()
        {
            const string bidKey = "bid-0808080808080808080808080808080808080808080808080808080808080808";
            var key = GlobalStateKey.FromString(bidKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BidKey);
            Assert.AreEqual(bidKey, key.ToString());

            var bytes = Hex.Decode("070808080808080808080808080808080808080808080808080808080808080808");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BidKey);
            Assert.AreEqual(bidKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void WithdrawKeyTest()
        {
            const string withdrawKey = "withdraw-0909090909090909090909090909090909090909090909090909090909090909";
            var key = GlobalStateKey.FromString(withdrawKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is WithdrawKey);
            Assert.AreEqual(withdrawKey, key.ToString());

            var bytes = Hex.Decode("080909090909090909090909090909090909090909090909090909090909090909");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is WithdrawKey);
            Assert.AreEqual(withdrawKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void DictionaryKeyTest()
        {
            const string dictionaryKey = "dictionary-1010101010101010101010101010101010101010101010101010101010101010";
            var key = GlobalStateKey.FromString(dictionaryKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DictionaryKey);
            Assert.AreEqual(dictionaryKey, key.ToString());

            var bytes = Hex.Decode("091010101010101010101010101010101010101010101010101010101010101010");
            key = GlobalStateKey.FromBytes(bytes);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DictionaryKey);
            Assert.AreEqual(dictionaryKey, key.ToString());
            Assert.IsTrue(key.GetBytes().SequenceEqual(bytes));
        }

        [Test]
        public void InvalidPrefixTest()
        {
            const string invalidPrefixKey = "invalid-prefix-0102030405060708";

            var ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new AccountHashKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new HashKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new URef(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new TransferKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new DeployInfoKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new EraInfoKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new BalanceKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new BidKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new WithdrawKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
                var key = new DictionaryKey(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. It should start with"));
            
            ex = Assert.Catch<ArgumentException>(() =>
            {
               GlobalStateKey.FromString(invalidPrefixKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Key not valid. Unknown key prefix."));
        }

        [Test]
        public void InvalidKeyIdentifierTest()
        {
            var bytes = Hex.Decode("0F00");
            var ex = Assert.Catch<ArgumentException>(() =>
            {
                GlobalStateKey.FromBytes(bytes);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Unknown key identifier"));   
        }

        [Test]
        public void CEP57ChecksumValidationTest()
        {
            var hashKey = "hash-98d945f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6";

            var key = GlobalStateKey.FromString(hashKey);
            Assert.IsNotNull(key);

            var invalidHashKey = "hash-98D945F5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6";
            var ex = Assert.Catch<ArgumentException>(() =>
            {
                GlobalStateKey.FromString(invalidHashKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("Global State Key checksum mismatch."));

            var urefKey = "uref-98d945f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6-007";
            key = GlobalStateKey.FromString(urefKey);
            Assert.IsNotNull(key);
            
            var invalidURefKey = "uref-98D945F5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6-007";
            ex = Assert.Catch<ArgumentException>(() =>
            {
                GlobalStateKey.FromString(invalidURefKey);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.StartsWith("URef checksum mismatch."));
        }
        
        [Test]
        public void HashKeyJsonDeserializeTest()
        {
            const string json = @"{""key"":""hash-98d945f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6"",""kind"":""Read""}";
            var op = JsonSerializer.Deserialize<Operation>(json);
            Assert.IsNotNull(op);
            
            const string invalidJson = @"{""key"":""hash-aaaaa5f5324F865243B7c02C0417AB6eaC361c5c56602FD42ced834a1Ba201B6"",""kind"":""Read""}";
            var ex = Assert.Catch(() => JsonSerializer.Deserialize<Operation>(invalidJson));
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("checksum mismatch"));
        }
    }
}
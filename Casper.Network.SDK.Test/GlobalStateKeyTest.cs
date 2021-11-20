using System;
using System.Text.Json;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class GlobalStateKeyTest
    {
        private JsonSerializerOptions serializerOptions;
        
        [SetUp]
        public void SetUp()
        {
            serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = false,
                Converters =
                {
                    new GlobalStateKey.GlobalStateKeyConverter()
                }
            };
        }
        
        [Test]
        public void AccountHashTest()
        {
            const string accountHash = "account-hash-0101010101010101010101010101010101010101010101010101010101010101";
            var key = GlobalStateKey.FromString(accountHash);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is AccountHashKey);
            Assert.AreEqual(accountHash, key.ToString());
            
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
        }

        [Test]
        public void TransferKeyTest()
        {
            const string transferKey = "transfer-0404040404040404040404040404040404040404040404040404040404040404";
            var key = GlobalStateKey.FromString(transferKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is TransferKey);
            Assert.AreEqual(transferKey, key.ToString());            
        }

        [Test]
        public void DeployInfoKeyTest()
        {
            const string deployKey = "deploy-0505050505050505050505050505050505050505050505050505050505050505";
            var key = GlobalStateKey.FromString(deployKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DeployInfoKey);
            Assert.AreEqual(deployKey, key.ToString());            
        }

        [Test]
        public void EraInfoKeyTest()
        {
            const string eraInfoKey = "era-12345";
            var key = GlobalStateKey.FromString(eraInfoKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is EraInfoKey);
            Assert.AreEqual(eraInfoKey, key.ToString());            
        }

        [Test]
        public void BalanceKeyTest()
        {
            const string balanceKey = "balance-0606060606060606060606060606060606060606060606060606060606060606";
            var key = GlobalStateKey.FromString(balanceKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BalanceKey);
            Assert.AreEqual(balanceKey, key.ToString());            
        }

        [Test]
        public void BidKeyTest()
        {
            const string bidKey = "bid-0707070707070707070707070707070707070707070707070707070707070707";
            var key = GlobalStateKey.FromString(bidKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is BidKey);
            Assert.AreEqual(bidKey, key.ToString());            
        }

        [Test]
        public void WithdrawKeyTest()
        {
            const string withdrawKey = "withdraw-0808080808080808080808080808080808080808080808080808080808080808";
            var key = GlobalStateKey.FromString(withdrawKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is WithdrawKey);
            Assert.AreEqual(withdrawKey, key.ToString());            
        }

        [Test]
        public void DictionaryKeyTest()
        {
            const string dictionaryKey = "dictionary-0909090909090909090909090909090909090909090909090909090909090909";
            var key = GlobalStateKey.FromString(dictionaryKey);
            Assert.IsNotNull(key);
            Assert.IsTrue(key is DictionaryKey);
            Assert.AreEqual(dictionaryKey, key.ToString());            
        }

        [Test]
        public void InvalidKeyTest()
        {
            const string invalidPrefixKey = "invalid-prefix-0102030405060708";

            try
            {
                var key = new AccountHashKey(invalidPrefixKey);
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Key not valid. It should start with"));
            }
            catch (Exception e)
            {
                Assert.Fail("ArgumentException expected but received: " + e.Message);
            }
        }
    }
}
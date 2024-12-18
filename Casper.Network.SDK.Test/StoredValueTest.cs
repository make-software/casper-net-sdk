using System.IO;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NetCasperTest
{
    public class StoredValueTest
    {
        [Test]
        public void SeigniorageAllocationsTest_V200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/seigniorage_allocation_v200.json");

            var storedValue = JsonSerializer.Deserialize<StoredValue>(json);
            Assert.IsNotNull(storedValue);
            Assert.IsNotNull(storedValue.EraInfo);
            Assert.AreEqual(20, storedValue.EraInfo.SeigniorageAllocations.Count);
            Assert.IsTrue(storedValue.EraInfo.SeigniorageAllocations[0].IsDelegator);
            Assert.AreEqual("018b46617b2b97e633b36530f2964b3f4c15916235910a2737e83d4fa2c7fad542", storedValue.EraInfo.SeigniorageAllocations[0].DelegatorKind.PublicKey.ToString().ToLower());
            Assert.AreEqual("01509254f22690fbe7fb6134be574c4fbdb060dfa699964653b99753485e518ea6", storedValue.EraInfo.SeigniorageAllocations[0].ValidatorPublicKey.ToString().ToLower());
            Assert.AreEqual("2515330120214391", storedValue.EraInfo.SeigniorageAllocations[0].Amount.ToString());
            Assert.IsFalse(storedValue.EraInfo.SeigniorageAllocations[1].IsDelegator);
        }
        
        [Test]
        public void SeigniorageAllocationsTest_V158()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/seigniorage_allocation_v158.json");

            var storedValue = JsonSerializer.Deserialize<StoredValue>(json);
            Assert.IsNotNull(storedValue);
            Assert.IsNotNull(storedValue.EraInfo);
            Assert.AreEqual(10, storedValue.EraInfo.SeigniorageAllocations.Count);
            Assert.IsTrue(storedValue.EraInfo.SeigniorageAllocations[0].IsDelegator);
            Assert.AreEqual("018b46617b2b97e633b36530f2964b3f4c15916235910a2737e83d4fa2c7fad542", storedValue.EraInfo.SeigniorageAllocations[0].DelegatorKind.PublicKey.ToString().ToLower());
            Assert.AreEqual("01509254f22690fbe7fb6134be574c4fbdb060dfa699964653b99753485e518ea6", storedValue.EraInfo.SeigniorageAllocations[0].ValidatorPublicKey.ToString().ToLower());
            Assert.AreEqual("4414692857142857", storedValue.EraInfo.SeigniorageAllocations[0].Amount.ToString());
            Assert.IsFalse(storedValue.EraInfo.SeigniorageAllocations[1].IsDelegator);
        }
    }
}
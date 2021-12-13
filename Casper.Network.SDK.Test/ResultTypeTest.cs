using System;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class ResultTypeTest
    {
        [Test]
        public void OkInt32StringTest()
        {
            var r1 = Result<Int32, string>.Ok(int.MaxValue);
            Assert.IsNotNull(r1);
            Assert.IsTrue(r1.Success);
            Assert.IsFalse(r1.IsFailure);
            Assert.IsNull(r1.Error);
            int value = r1.Value;
            Assert.AreEqual(int.MaxValue, value);
        }
        
        [Test]
        public void OkInt32Int32Test()
        {
            var r1 = Result<Int32, Int32>.Ok(int.MaxValue);
            Assert.IsNotNull(r1);
            Assert.IsTrue(r1.Success);
            Assert.IsFalse(r1.IsFailure);
            Assert.AreEqual(default(int), r1.Error);
            int value = r1.Value;
            Assert.AreEqual(int.MaxValue, value);
        }
        
        [Test]
        public void OkUnitStringTest()
        {
            var r1 = Result<Unit, String>.Ok(Unit.Default);
            Assert.IsNotNull(r1);
            Assert.IsTrue(r1.Success);
            Assert.IsFalse(r1.IsFailure);
            Assert.AreEqual(null, r1.Error);
            Assert.AreEqual(Unit.Default, r1.Value);
        }
        
        [Test]
        public void FailInt32StringTest()
        {
            var r1 = Result<Int32, String>.Fail("failure message");
            Assert.IsNotNull(r1);
            Assert.IsFalse(r1.Success);
            Assert.IsTrue(r1.IsFailure);
            Assert.AreEqual(default(int), r1.Value);
            Assert.AreEqual("failure message", r1.Error);
        }

        [Test]
        public void FailUnitStringTest()
        {
            var r1 = Result<Unit, String>.Fail("failure message");
            Assert.IsNotNull(r1);
            Assert.IsFalse(r1.Success);
            Assert.IsTrue(r1.IsFailure);
            Assert.AreEqual(null, r1.Value);
            Assert.AreEqual("failure message", r1.Error);
        }
    }
}
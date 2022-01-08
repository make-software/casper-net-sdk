using System;
using System.Globalization;
using Casper.Network.SDK.Utils;
using NUnit.Framework;

namespace NetCasperTest
{
    public class DateUtilsTest
    {
        private ulong timestamp = 1640908800555;
        private string isotime = "2021-12-31T00:00:00.555Z";
        
        [Test]
        public void ToEpochTimeTest()
        {
            var t1 = DateUtils.ToEpochTime(isotime);
            Assert.AreEqual(timestamp, t1);

            var date = new DateTime(2021, 12, 31, 0, 0, 0, 555);
            var t3 = DateUtils.ToEpochTime(date);
            Assert.AreEqual(timestamp, t3);
        }

        [Test]
        public void ToIsoStringTest()
        {
            var date1 = DateUtils.ToISOString(1640908800555);
            Assert.AreEqual("2021-12-31T00:00:00.555Z", date1);
        }
    }
}
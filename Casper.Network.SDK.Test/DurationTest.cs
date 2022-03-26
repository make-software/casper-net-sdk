using System;
using Casper.Network.SDK.Utils;
using NUnit.Framework;

namespace NetCasperTest
{
    public class DurationTest
    {
        [Test]
        public void MillisecondsToDurationTest()
        {
            Assert.AreEqual("31ms", DurationUtils.MillisecondsToString(31));
            Assert.AreEqual("3s", DurationUtils.MillisecondsToString(3000));
            Assert.AreEqual("30s", DurationUtils.MillisecondsToString(30_000));
            Assert.AreEqual("1m", DurationUtils.MillisecondsToString(60_000));
            Assert.AreEqual("10m", DurationUtils.MillisecondsToString(600_000));
            Assert.AreEqual("30m", DurationUtils.MillisecondsToString(1_800_000));
            Assert.AreEqual("2h", DurationUtils.MillisecondsToString(7_200_000));
            Assert.AreEqual("1day", DurationUtils.MillisecondsToString(86_400_000));
            Assert.AreEqual("2days", DurationUtils.MillisecondsToString(172_800_000));
            Assert.AreEqual("7days", DurationUtils.MillisecondsToString(604_800_000));
            Assert.AreEqual("1month", DurationUtils.MillisecondsToString(86_400_000d * 30.44d));
            Assert.AreEqual("2months", DurationUtils.MillisecondsToString(172_800_000 * 30.44d));
            Assert.AreEqual("1year", DurationUtils.MillisecondsToString(86_400_000d * 365.25d));
            Assert.AreEqual("2years", DurationUtils.MillisecondsToString(172_800_000 * 365.25d));

            Assert.AreEqual("2days 2h 30m 10s 31ms",
                DurationUtils.MillisecondsToString(172_800_000
                                                   + 7_200_000
                                                   + 1_800_000
                                                   + 10_000
                                                   + 31));

            Assert.AreEqual("2years 7months 10s 31ms",
                DurationUtils.MillisecondsToString(172_800_000 * 365.25d
                                                   + 7 * 86_400_000d * 30.44d
                                                   + 10_000
                                                   + 31));

            Assert.AreEqual("0s", DurationUtils.MillisecondsToString(0));
        }

        [Test]
        public void DurationToMillisecondsTest()
        {
            Assert.AreEqual(172_800_000
                            + 7_200_000
                            + 1_800_000
                            + 10_000
                            + 31,
                DurationUtils.StringToMilliseconds("2days 2h 30m 10s 31ms"));

            Assert.AreEqual(172_800_000 * 365.25d
                            + 7 * 86_400_000d * 30.44d
                            + 86_400_000
                            + 10_000
                            + 31,
                DurationUtils.StringToMilliseconds("2years 7 months 1 day 10s 31ms"));

            Assert.AreEqual(7 * 24 * 3600 * 1000,
                DurationUtils.StringToMilliseconds("1week"));
            Assert.AreEqual(7 * 24 * 3600 * 1000,
                DurationUtils.StringToMilliseconds("1weeks"));
            Assert.AreEqual(3 * 7 * 24 * 3600 * 1000,
                DurationUtils.StringToMilliseconds("3weeks"));

            Assert.AreEqual(0, DurationUtils.StringToMilliseconds("0years 0days 0s"));
        }

        [Test]
        public void CatchWrongDurationTest()
        {
            var ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.MillisecondsToString(-1000d));
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("Negative values not allowed."));

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.MillisecondsToString(double.MaxValue));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("584542047years"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("99999999999999999999999999ms"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("-1ms"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("ms"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("xms"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("7 eras"));
            Assert.IsNotNull(ex);

            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("123"));
            Assert.IsNotNull(ex);
            
            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("1m 7"));
            Assert.IsNotNull(ex);
            
            ex = Assert.Catch<ArgumentOutOfRangeException>(() =>
                DurationUtils.StringToMilliseconds("1m 7 1ms"));
            Assert.IsNotNull(ex);
        }
    }
}
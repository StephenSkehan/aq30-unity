using System;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class TimeProviderTests
    {
        [Test]
        public void FixedNow_IsStable()
        {
            var fixedNow = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);
            var tp = new AQ.SharedKernel.FixedTimeProvider(fixedNow);

            var t = tp.GetType();
            var nowProp = t.GetProperty("Now") ?? t.GetProperty("UtcNow") ?? t.GetProperty("NowUtc");
            Assert.NotNull(nowProp, "FixedTimeProvider needs Now/UtcNow/NowUtc property.");

            var t1 = (DateTime)nowProp!.GetValue(tp)!;
            var t2 = (DateTime)nowProp.GetValue(tp)!;

            Assert.AreEqual(fixedNow, t1);
            Assert.AreEqual(t1, t2);
        }
    }
}

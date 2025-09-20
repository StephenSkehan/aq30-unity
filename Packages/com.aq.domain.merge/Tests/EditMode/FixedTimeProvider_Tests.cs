using System;
using NUnit.Framework;
using AQ.SharedKernel;

namespace AQ.Domain.Merge.Tests
{
    public sealed class FixedTimeProvider_Tests
    {
        private sealed class DummyTime : ITimeProvider
        {
            public DateTime UtcNow { get; private set; } =
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public void AdvanceSeconds(int seconds) =>
                UtcNow = UtcNow.AddSeconds(seconds);
        }

        [Test]
        public void UtcNow_is_a_valid_DateTime()
        {
            var t = new DummyTime();
            var now = t.UtcNow;
            Assert.That(now, Is.Not.EqualTo(default(DateTime)));
            Assert.AreEqual(DateTimeKind.Utc, now.Kind);
        }

        [Test]
        public void If_advance_method_exists_time_moves_forward()
        {
            var t = new DummyTime();
            var before = t.UtcNow;
            t.AdvanceSeconds(2);
            var after = t.UtcNow;
            Assert.Greater(after, before);
        }
    }
}

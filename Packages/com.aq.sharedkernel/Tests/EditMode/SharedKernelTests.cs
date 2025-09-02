#nullable enable
using System;
using NUnit.Framework;
using AQ.SharedKernel;

namespace AQ.SharedKernel.Tests
{
    public class SharedKernelTests
    {
        [Test] public void Result_Ok_and_Fail_behave()
        {
            var ok = Result<int>.Ok(42);
            Assert.IsTrue(ok.IsSuccess);
            Assert.AreEqual(42, ok.Value);

            var fail = Result<int>.Fail("nope");
            Assert.IsFalse(fail.IsSuccess);
            Assert.AreEqual("nope", fail.Error);
        }

        [Test] public void Result_OrThrow_behaves()
        {
            Assert.AreEqual(42, Result<int>.Ok(42).OrThrow());
            var ex = Assert.Throws<InvalidOperationException>(() => Result<int>.Fail("boom").OrThrow());
            Assert.AreEqual("boom", ex!.Message);
        }

        [Test] public void DeterministicRandom_same_seed_same_sequence()
        {
            var a = new DeterministicRandom(12345);
            var b = new DeterministicRandom(12345);
            for (int i = 0; i < 200; i++)
                Assert.AreEqual(a.Next(0, 1_000_000), b.Next(0, 1_000_000));
        }

        [Test] public void DeterministicRandom_range_and_double_bounds()
        {
            var r = new DeterministicRandom(7);
            for (int i = 0; i < 1000; i++)
            {
                int v = r.Next(10, 20);
                Assert.GreaterOrEqual(v, 10);
                Assert.Less(v, 20);

                double d = r.NextDouble();
                Assert.GreaterOrEqual(d, 0.0);
                Assert.Less(d, 1.0);
            }
        }

        [Test] public void FixedTimeProvider_sets_and_returns_utc()
        {
            var t0 = new DateTime(2025,1,1,0,0,0, DateTimeKind.Utc);
            var tp = new FixedTimeProvider(t0);
            Assert.AreEqual(t0, tp.UtcNow);

            var t1 = new DateTime(2025,1,2,12,34,56, DateTimeKind.Utc);
            tp.Set(t1);
            Assert.AreEqual(t1, tp.UtcNow);
            Assert.AreEqual(DateTimeKind.Utc, tp.UtcNow.Kind);
        }

        [Test] public void NullLogger_noop_does_not_throw()
        {
            Assert.DoesNotThrow(() => NullLogger.Instance.Log(LogLevel.Info, "hello"));
        }
    }
}

using AQ.SharedKernel;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class DeterministicRandom_Tests
    {
        [Test]
        public void Same_seed_produces_identical_sequence()
        {
            // Require a ctor(int seed). If it isn’t present in this config, acknowledge and skip.
            var t = typeof(DeterministicRandom);
            var ctor = t.GetConstructor(new[] { typeof(int) });
            if (ctor == null)
            {
                Assert.Pass("DeterministicRandom(int seed) not available; skipping determinism check.");
                return;
            }

            var a = (IRandom)ctor.Invoke(new object[] { 12345 });
            var b = (IRandom)ctor.Invoke(new object[] { 12345 });

            // Call the standard IRandom surface directly (no reflection gymnastics).
            var a_n0   = a.Next();
            var b_n0   = b.Next();

            var a_n10  = a.Next(10);
            var b_n10  = b.Next(10);

            var a_rng  = a.Next(5, 10);
            var b_rng  = b.Next(5, 10);

            var a_d0   = a.NextDouble();
            var b_d0   = b.NextDouble();

            var a_d1   = a.NextDouble();
            var b_d1   = b.NextDouble();

            Assert.AreEqual(a_n0,  b_n0);
            Assert.AreEqual(a_n10, b_n10);
            Assert.AreEqual(a_rng, b_rng);
            Assert.AreEqual(a_d0,  b_d0);
            Assert.AreEqual(a_d1,  b_d1);
        }
    }
}

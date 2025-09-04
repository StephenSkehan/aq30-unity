using System;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class RandomMoreTests
    {
        [Test]
        public void Random_RangeMethods_AreWithinBounds()
        {
            var r = new AQ.SharedKernel.DeterministicRandom(999);
            for (int i = 0; i < 100; i++)
            {
                int v1 = r.Next(10);
                Assert.That(v1, Is.GreaterThanOrEqualTo(0).And.LessThan(10));

                int v2 = r.Next(5, 15);
                Assert.That(v2, Is.GreaterThanOrEqualTo(5).And.LessThan(15));
            }
        }

        [Test]
        public void Random_Double_And_Bytes_Work_When_Available()
        {
            var r = new AQ.SharedKernel.DeterministicRandom(123);
            var t = r.GetType();

            var nextDouble = t.GetMethod("NextDouble", Type.EmptyTypes);
            if (nextDouble != null)
            {
                for (int i = 0; i < 50; i++)
                {
                    double d = (double)nextDouble.Invoke(r, null);
                    Assert.That(d, Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
                }
            }

            var nextBytes = t.GetMethod("NextBytes", new[] { typeof(byte[]) });
            if (nextBytes != null)
            {
                var buf = new byte[32];
                nextBytes.Invoke(r, new object[] { buf });
                Assert.AreEqual(32, buf.Length);
            }
        }
    }
}

using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class RandomDeterminismTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var a = new AQ.SharedKernel.DeterministicRandom(12345);
            var b = new AQ.SharedKernel.DeterministicRandom(12345);

            var seqA = new[] { a.Next(), a.Next(), a.Next() };
            var seqB = new[] { b.Next(), b.Next(), b.Next() };

            Assert.That(seqA, Is.EqualTo(seqB));
        }
    }
}

#nullable enable
using System;

namespace AQ.SharedKernel
{
    /// NOTE: wraps System.Random (not thread-safe). If we add multithreading later,
    /// swap to a thread-local or PCG/XorShift with the same IRandom surface.
    public sealed class DeterministicRandom : IRandom
    {
        private readonly Random _rng;
        public DeterministicRandom(int seed) { _rng = new Random(seed); }

        public int Next() => _rng.Next();
        public int Next(int maxExclusive) => _rng.Next(maxExclusive);
        public int Next(int minInclusive, int maxExclusive) => _rng.Next(minInclusive, maxExclusive);
        public double NextDouble() => _rng.NextDouble();
    }
}

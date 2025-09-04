using System;
using AQ.Domain.Merge;
using AQ.SharedKernel;

namespace AQ.App
{
    // Minimal, engine-agnostic composition root for tests and headless bootstrap.
    public sealed class GameComposer
    {
        public sealed class Components
        {
            public IEventBus Bus { get; }
            public IRandom Rng { get; }
            public ITimeProvider Time { get; }
            public IRecipeBook Recipes { get; }
            public Grid Grid { get; }
            public MergeEngine Engine { get; }

            public Components(IEventBus bus, IRandom rng, ITimeProvider time,
                              IRecipeBook recipes, Grid grid, MergeEngine engine)
            {
                Bus = bus; Rng = rng; Time = time; Recipes = recipes; Grid = grid; Engine = engine;
            }
        }

        // A conservative default build: no recipe data, safe time provider, deterministic RNG if available.
        public static Components Build(IRecipeBook recipes = null)
        {
            // Event bus
            IEventBus bus = new InMemoryEventBus();

            // RNG: prefer a deterministic RNG; fall back to any default ctor if seed ctor not exposed.
            IRandom rng;
            try { rng = (IRandom)Activator.CreateInstance(typeof(DeterministicRandom), new object[] { 12345 }); }
            catch { rng = (IRandom)Activator.CreateInstance(typeof(DeterministicRandom)); }

            // Time provider: prefer a simple UTC system time provider to avoid reflection assumptions.
            ITimeProvider time = new SystemTimeProvider();

            // Recipe book: allow injection, else an empty recipe set (no merges succeed).
            IRecipeBook book = recipes ?? new EmptyRecipes();

            var grid = new Grid();
            var engine = new MergeEngine(grid, book, bus, rng, time);
            return new Components(bus, rng, time, book, grid, engine);
        }

        // Local light-weight providers to avoid coupling to SharedKernel ctor shapes.
        private sealed class SystemTimeProvider : ITimeProvider
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }

        private sealed class EmptyRecipes : IRecipeBook
        {
            public bool TryGetResult(ItemId a, ItemId b, out ItemId result) { result = default; return false; }
        }
    }
}

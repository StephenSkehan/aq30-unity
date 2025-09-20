using System;
using System.Collections.Generic;
using AQ.SharedKernel;

namespace AQ.Domain.Merge
{
    public sealed class MergeEngine
    {
        private readonly IGrid _grid;
        private readonly IRecipeBook _recipes;
        private readonly IEventBus _bus;
        private readonly IRandom _rng;
        private readonly ITimeProvider _time;

        private readonly Dictionary<int, DateTime> _cooldowns = new();
        private readonly TimeSpan _mergeCooldown = TimeSpan.FromSeconds(1);

        // Primary ctor
        public MergeEngine(IGrid grid, IRecipeBook recipes, IEventBus bus, IRandom rng, ITimeProvider time)
        {
            _grid = grid; _recipes = recipes; _bus = bus; _rng = rng; _time = time;
        }

        // Back-compat helpers & overloads
        private sealed class FallbackRandom : IRandom
        {
            private readonly Random _r = new Random(12345);
            public int    Next() => _r.Next();
            public int    Next(int maxValue) => _r.Next(maxValue);
            public int    Next(int minValue, int maxValue) => _r.Next(minValue, maxValue);
            public double NextDouble() => _r.NextDouble();
        }
        private sealed class SystemTime : ITimeProvider { public DateTime UtcNow => DateTime.UtcNow; }
        private sealed class NullBus : IEventBus
        {
            public void Publish<T>(T e) where T : AQ.SharedKernel.Events.IGameEvent { }
            public IDisposable Subscribe<T>(Action<T> handler) where T : AQ.SharedKernel.Events.IGameEvent => new Noop();
            private sealed class Noop : IDisposable { public void Dispose() { } }
        }

        public MergeEngine(IGrid grid, IRecipeBook recipes, IEventBus bus)
            : this(grid, recipes, bus, new FallbackRandom(), new SystemTime()) { }

        public MergeEngine(IGrid grid, IRecipeBook recipes)
            : this(grid, recipes, new NullBus(), new FallbackRandom(), new SystemTime()) { }

        // Older test patterns:
        public MergeEngine(IRecipeBook recipes, IEventBus bus)
            : this(new Grid(), recipes, bus, new FallbackRandom(), new SystemTime()) { }

        public MergeEngine(IEventBus bus, IRecipeBook recipes)
            : this(new Grid(), recipes, bus, new FallbackRandom(), new SystemTime()) { }

        // --- NEW: non-grid overload to merge two items directly (for tests that pass ItemIds) ---
        public Result<ItemId> TryMerge(ItemId left, ItemId right)
        {
            if (!left.Equals(right))
                return Result<ItemId>.Fail("Items are not merge-compatible (different kinds).");

            if (!_recipes.TryGetResult(left, right, out var merged))
                return Result<ItemId>.Fail("No recipe exists for the requested merge.");

            // Determinism seam (not used yet)
            var _ = _rng.NextDouble();

            // Indices are unknown in this overload; use -1 to indicate N/A.
            var evt = EventHelpers.TryCreateMergePerformed(-1, -1, left, right, merged);
            EventHelpers.PublishDynamic(_bus, evt);

            return Result<ItemId>.Ok(merged);
        }

        // Grid-based merge (indices)
        public Result<ItemId> TryMerge(int fromIndex, int toIndex)
        {
            var now = _time.UtcNow;

            if (_cooldowns.TryGetValue(fromIndex, out var aReady) && now < aReady)
                return Result<ItemId>.Fail("Source cell is on cooldown.");
            if (_cooldowns.TryGetValue(toIndex, out var bReady) && now < bReady)
                return Result<ItemId>.Fail("Target cell is on cooldown.");

            var a = GridOps.GetCell(_grid, fromIndex);
            var b = GridOps.GetCell(_grid, toIndex);

            if (!a.Equals(b))
                return Result<ItemId>.Fail("Items are not merge-compatible (different kinds).");

            if (!_recipes.TryGetResult(a, b, out var merged))
                return Result<ItemId>.Fail("No recipe exists for the requested merge.");

            GridOps.SetCell(_grid, fromIndex, default);
            GridOps.SetCell(_grid, toIndex,   merged);

            var readyAt = now + _mergeCooldown;
            _cooldowns[fromIndex] = readyAt;
            _cooldowns[toIndex]   = readyAt;

            var _ = _rng.NextDouble(); // deterministic seam

            var evt = EventHelpers.TryCreateMergePerformed(fromIndex, toIndex, a, b, merged);
            EventHelpers.PublishDynamic(_bus, evt);

            return Result<ItemId>.Ok(merged);
        }

        // Back-compat convenience for older tests
        public bool TryMerge(int fromIndex, int toIndex, out ItemId result)
        {
            var r = TryMerge(fromIndex, toIndex);
            result = r.IsSuccess ? r.Value : default;
            return r.IsSuccess;
        }
    }
}

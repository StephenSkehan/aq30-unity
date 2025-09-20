using System;
using System.Collections.Generic;
using AQ.SharedKernel;
using AQ.SharedKernel.Events;
using AQ.SharedKernel.Events;

namespace AQ.Domain.Merge.Tests
{
    // Time..
    internal sealed class FixedTime : ITimeProvider
    {
        public DateTime UtcNow { get; private set; } = new DateTime(2000,1,1,0,0,0,DateTimeKind.Utc);
        public void AdvanceSeconds(int seconds) => UtcNow = UtcNow.AddSeconds(seconds);
    }

    // RNG
    internal sealed class FixedRandom : IRandom
    {
        private readonly double[] _values;
        private int _i;
        public FixedRandom(params double[] values) { _values = (values != null && values.Length > 0) ? values : new double[] { 0.5 }; }
        public int Next() => 0;
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => _values[_i++ % _values.Length];
    }

    // Event buses
    internal sealed class EventBusSpy : AQ.SharedKernel.Events.IEventBus, AQ.SharedKernel.IEventBus
    {
        public readonly List<object> Published = new List<object>();
        public void Publish<T>(T e) where T : AQ.SharedKernel.Events.IGameEvent => Published.Add(e!);
        public IDisposable Subscribe<T>(Action<T> handler) where T : AQ.SharedKernel.Events.IGameEvent => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() {} }
    }

    internal sealed class TestBus : AQ.SharedKernel.Events.IEventBus, AQ.SharedKernel.IEventBus
    {
        public int Count;
        public void Publish<T>(T e) where T : AQ.SharedKernel.Events.IGameEvent => Count++;
        public IDisposable Subscribe<T>(Action<T> handler) where T : AQ.SharedKernel.Events.IGameEvent => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() {} }
    }

    // Recipe books
    internal sealed class RecipeBookTable : IRecipeBook
    {
        private readonly Dictionary<(ItemId, ItemId), ItemId> _map = new Dictionary<(ItemId, ItemId), ItemId>();
        public void Add(ItemId a, ItemId b, ItemId result)
        {
            _map[(a, b)] = result;
            _map[(b, a)] = result;
        }
        public bool TryGetResult(ItemId a, ItemId b, out ItemId result) => _map.TryGetValue((a, b), out result);
    }

    internal sealed class RecipeBookPair : IRecipeBook
    {
        private readonly ItemId _input, _output;
        public RecipeBookPair(ItemId input, ItemId output) { _input = input; _output = output; }
        public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
        {
            if (a.Equals(_input) && b.Equals(_input)) { result = _output; return true; }
            result = default; return false;
        }
    }
}

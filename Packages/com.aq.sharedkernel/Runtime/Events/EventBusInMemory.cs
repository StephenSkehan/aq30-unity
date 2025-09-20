using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AQ.SharedKernel.Events
{
    // Thread-safe, GC-friendly in-memory bus (no Unity dependency)
    public sealed class EventBusInMemory : AQ.SharedKernel.Events.IEventBus, AQ.SharedKernel.IEventBus{
        private readonly ConcurrentDictionary<Type, List<Delegate>> _subs =
            new ConcurrentDictionary<Type, List<Delegate>>();

        public void Publish<T>(T evt) where T : IGameEvent
        {
            if (evt == null) return;
            var t = typeof(T);
            if (_subs.TryGetValue(t, out var list))
            {
                // Snapshot to avoid mutation issues during publish
                foreach (var d in list.ToArray())
                    (d as Action<T>)?.Invoke(evt);
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var t = typeof(T);
            var list = _subs.GetOrAdd(t, _ => new List<Delegate>());
            list.Add(handler);

            return new Unsub(() =>
            {
                if (_subs.TryGetValue(t, out var l))
                {
                    l.Remove(handler);
                    if (l.Count == 0) _subs.TryRemove(t, out _);
                }
            });
        }

        private sealed class Unsub : IDisposable
        {
            private Action _dispose;
            public Unsub(Action dispose) { _dispose = dispose; }
            public void Dispose() { _dispose?.Invoke(); _dispose = null; }
        }
    }
}
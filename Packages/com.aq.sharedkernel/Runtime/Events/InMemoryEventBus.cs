using System;
using System.Collections.Generic;

namespace AQ.SharedKernel
{
    public sealed class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers =
            new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            var key = typeof(T);
            List<Delegate> list;
            if (!_handlers.TryGetValue(key, out list))
            {
                list = new List<Delegate>();
                _handlers[key] = list;
            }
            list.Add(handler);
            return new Subscription(this, key, handler);
        }

        public void Publish<T>(T evt)
        {
            List<Delegate> list;
            if (_handlers.TryGetValue(typeof(T), out list))
            {
                // copy to avoid modification during iteration
                var snapshot = list.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                    ((Action<T>)snapshot[i])(evt);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly InMemoryEventBus _bus;
            private readonly Type _type;
            private readonly Delegate _handler;
            private bool _disposed;

            public Subscription(InMemoryEventBus bus, Type type, Delegate handler)
            {
                _bus = bus; _type = type; _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                List<Delegate> list;
                if (_bus._handlers.TryGetValue(_type, out list))
                {
                    list.Remove(_handler);
                    if (list.Count == 0) _bus._handlers.Remove(_type);
                }
                _disposed = true;
            }
        }
    }
}

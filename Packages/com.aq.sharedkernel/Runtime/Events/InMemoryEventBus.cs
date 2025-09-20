using System;
using AQ.SharedKernel.Events;

namespace AQ.SharedKernel
{
    // Adapter that preserves the old class name but delegates to the canonical Events.EventBusInMemory
    public sealed class InMemoryEventBus : IEventBus
    {
        private readonly EventBusInMemory _inner = new EventBusInMemory();

        public IDisposable Subscribe<T>(Action<T> handler) where T : AQ.SharedKernel.Events.IGameEvent
            => _inner.Subscribe(handler);

        public void Publish<T>(T evt) where T : AQ.SharedKernel.Events.IGameEvent
            => _inner.Publish(evt);
    }
}
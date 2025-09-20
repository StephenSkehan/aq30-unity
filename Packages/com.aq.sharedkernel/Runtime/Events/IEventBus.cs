using System;

namespace AQ.SharedKernel.Events
{
    // Marker interface for strongly-typed events
    public interface IGameEvent { }

    // Minimal event bus interface (engine-agnostic)
    public interface IEventBus
    {
        void Publish<T>(T evt) where T : IGameEvent;
        IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent;
    }
}
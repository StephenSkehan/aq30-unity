namespace AQ.SharedKernel
{
    // Namespace-compatibility shim:
    // Old code that references AQ.SharedKernel.IEventBus / IGameEvent continues to compile,
    // while new code uses AQ.SharedKernel.Events.*. The concrete bus will implement BOTH.
    public interface IGameEvent : AQ.SharedKernel.Events.IGameEvent { }
    public interface IEventBus  : AQ.SharedKernel.Events.IEventBus  { }
}
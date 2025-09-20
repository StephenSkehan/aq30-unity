namespace AQ.SharedKernel
{
    // Authoritative root logger abstraction expected by tests.
    public interface ILogger
    {
        // Intentionally empty; 2-arg Log(LogLevel,string) comes from extensions.
    }
}

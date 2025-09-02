#nullable enable
namespace AQ.SharedKernel
{
    public enum LogLevel { Trace, Debug, Info, Warn, Error }

    public interface ILogger { void Log(LogLevel level, string message); }

    public sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();
        private NullLogger() {}
        public void Log(LogLevel level, string message) { /* no-op by design */ }
    }
}

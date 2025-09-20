using AQ.SharedKernel.Logging;

namespace AQ.SharedKernel
{
    /// <summary>
    /// Compatibility extension so calls like logger.Log(LogLevel.Info, "msg")
    /// work when only `using AQ.SharedKernel;` is present.
    /// Targets the real AQ.SharedKernel.Logging.ILogger (no redeclarations).
    /// </summary>
    public static class LoggerExtensionsCompat
    {
        public static void Log(this ILogger logger, LogLevel level, string message)
        {
            // no-op for compile; you can route to your real logger if desired
        }
    }
}

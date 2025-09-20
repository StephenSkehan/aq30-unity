namespace AQ.SharedKernel
{
    public static class LoggerFactory
    {
        public static ILogger Null => NullLogger.Instance;
    }
}

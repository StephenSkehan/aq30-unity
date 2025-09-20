namespace AQ.SharedKernel
{
    // Authoritative NullLogger: convention tests expect a public static PROPERTY named Instance, declared as ILogger.
    public sealed class NullLogger : ILogger
    {
        public static ILogger Instance { get; } = new NullLogger();
        private NullLogger() { }
    }
}

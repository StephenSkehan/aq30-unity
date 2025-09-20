namespace AQ.SharedKernel
{
    // Root-level LogLevel used by SharedKernel tests
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        // Tests may reference Info; alias it to Information
        Information = 2,
        Info = Information,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }
}

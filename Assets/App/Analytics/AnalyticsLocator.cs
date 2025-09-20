namespace AQ.App.Analytics
{
    /// <summary>
    /// Tiny service-locator for IAnalytics.
    /// Back-compat: exposes .Instance for existing callers.
    /// </summary>
    public static class AnalyticsLocator
    {
        private static IAnalytics _current;

        // BACK-COMPAT (existing code calls AnalyticsLocator.Instance)
        public static IAnalytics Instance => _current;

        // New alias (use in new code)
        public static IAnalytics Current => _current;

        public static void Set(IAnalytics impl) => _current = impl;

        public static bool TryGet(out IAnalytics analytics)
        {
            analytics = _current;
            return analytics != null;
        }

        public static void Clear() => _current = null;
    }
}

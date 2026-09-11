namespace AQ.App
{
    /// <summary>
    /// Forwarding shim onto <see cref="GameFlags"/> — kept so existing call
    /// sites compile unchanged. The separate nar_flag_* store it used to own
    /// was one half of the two-store trap (see GameFlags doc); legacy keys
    /// migrate lazily on read. New code should call GameFlags directly.
    /// </summary>
    public static class NarrativeFlags
    {
        public static void Set(string flag)   => GameFlags.Set(flag);
        public static bool Has(string flag)   => GameFlags.Has(flag);
        public static void Clear(string flag) => GameFlags.Clear(flag);
    }
}

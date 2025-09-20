namespace AQ.App.Composition
{
    public static class BootstrapperRuntime
    {
        public static void WireDefaults(IMergeService existing = null)
        {
            // Intentionally minimal for compile success; expand later.
            // If needed later: if (existing == null) existing = new AQ.App.UI.MergeBoard.MergeServiceStub();
        }
    }
}

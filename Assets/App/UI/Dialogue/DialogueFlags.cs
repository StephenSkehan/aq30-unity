using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Forwarding shim onto <see cref="GameFlags"/> — kept so existing call
    /// sites (DialogueRunner setsFlag/requiresFlag, gate checks) compile
    /// unchanged. The separate dlg_flag_* store it used to own was one half of
    /// the two-store trap (see GameFlags doc); legacy keys migrate lazily on
    /// read. New code should call GameFlags directly.
    /// </summary>
    public static class DialogueFlags
    {
        public static void Set(string flag)   => GameFlags.Set(flag);
        public static bool Has(string flag)   => GameFlags.Has(flag);
        public static void Clear(string flag) => GameFlags.Clear(flag);

        /// <summary>Clear all flags (testing/new game). NOTE: has always been a
        /// full PlayerPrefs.DeleteAll — the QA-reset semantics callers rely on.</summary>
        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[DialogueFlags] Cleared all flags (PlayerPrefs.DeleteAll)");
        }
    }
}

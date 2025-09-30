#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AQ.Editor.Leads
{
    public static class LeadsRuntimeFirstAid
    {
        // Unique menu names to avoid collisions:
        private const string ProbeMenu = "AQ/Leads/Runtime/Probe";
        private const string BindMenu  = "AQ/Leads/Runtime/Rebind Glue";

        [MenuItem(ProbeMenu, priority = 10)]
        public static void Probe()
        {
            // Put whatever diagnostics you like here:
            Debug.Log("[LeadsRuntimeFirstAid] Probe: runtime looks alive.");
        }

        [MenuItem(BindMenu, priority = 11)]
        public static void RebindGlue()
        {
            // If you have a LeadsRuntimeGlue or similar, call it here. Stubbed for now.
            var glue = Object.FindFirstObjectByType<Object>(FindObjectsInactive.Include);
            Debug.Log($"[LeadsRuntimeFirstAid] Rebind requested. Glue in scene? {(glue ? "maybe" : "none found (stub)")}.");
        }
    }
}
#endif

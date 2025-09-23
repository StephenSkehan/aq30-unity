// Assets/Editor/AQ/Debug/ListAdvanceWiring.cs
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Debugging
{
    /// <summary>
    /// Lists CaseFlow advance wiring in the open scenes.
    /// Updates API to FindObjectsByType to avoid deprecation warnings.
    /// </summary>
    public static class ListAdvanceWiring
    {
        [MenuItem("AQ/Debug/List Advance Wiring")]
        public static void List()
        {
            // Include inactive; no sorting (fast)
#if UNITY_2022_2_OR_NEWER
            var comps = Object.FindObjectsByType<AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            // Fallback for older Unity just in case
            var comps = Object.FindObjectsOfType<AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB>(true);
#endif

            if (comps == null || comps.Length == 0)
            {
                UnityEngine.Debug.Log("[AdvanceWiring] No CaseFlowAdvanceOnEventMB found in loaded scenes.");
                return;
            }

            var lines = comps
                .Where(c => c != null && c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded)
                .Select(c => $"{c.gameObject.scene.name}/{GetPath(c.transform)} → enabled:{c.enabled}");

            UnityEngine.Debug.Log("[AdvanceWiring] Found:\n - " + string.Join("\n - ", lines));
        }

        private static string GetPath(Transform t)
        {
            System.Collections.Generic.List<string> parts = new();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif

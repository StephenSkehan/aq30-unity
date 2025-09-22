// AQ — TMP Find Glyphs (runtime logger, Unity 6-safe)
// Menu: AQ → UI → TMP → Log Offenders (Runtime)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.UI
{
    public static class TMPFindGlyphs
    {
        [MenuItem("AQ/UI/TMP/Log Offenders (Runtime)")]
        public static void LogRuntimeOffenders()
        {
            // Unity 6: use FindObjectsByType with explicit sort mode.
            var tmps = Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;
            foreach (var t in tmps)
            {
                var s = t.text;
                if (!string.IsNullOrEmpty(s) && (s.Contains("✓") || s.Contains("\\u2713")))
                {
                    Debug.LogWarning($"[AQ TMP] Offender: {GetPath(t.gameObject)} → \"{TrimShow(s)}\"", t);
                    count++;
                }
            }
            Debug.Log(count > 0 ? $"[AQ TMP] Found {count} offender(s)." : "[AQ TMP] No offenders in live objects.");
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var p = go.transform.parent;
            while (p != null) { path = p.name + "/" + path; p = p.parent; }
            return path;
        }
        private static string TrimShow(string s)
        {
            s = s.Replace("\n"," ").Replace("\r"," ");
            return s.Length > 80 ? s.Substring(0,80)+"…" : s;
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace AQ.Editor.Diagnostics
{
    public static class FindDotItem
    {
        // Menu: scan scene(s) for TMP labels exactly "• Item" or "Item"
        [MenuItem("AQ/Diag/Find all '• Item' labels")]
        public static void FindAll()
        {
#if UNITY_2022_2_OR_NEWER
            var all = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            // Fallback for older Unity; may show deprecation warnings on newer versions
            var all = Object.FindObjectsOfType<TMP_Text>(true);
#endif
            int hits = 0;
            foreach (var tmp in all)
            {
                if (!tmp) continue;
                var s = (tmp.text ?? string.Empty).Trim();
                if (s == "• Item" || s == "Item")
                {
                    hits++;
                    var path = GetPath(tmp.transform);
                    Debug.Log($"🔎 Found '{s}' at: {path}", tmp);
                }
            }
            Debug.Log(hits == 0 ? "✅ No '• Item' labels in scene." : $"🧭 Total matches: {hits}");
        }

        // Menu: delete all placeholders (undoable)
        [MenuItem("AQ/Diag/Delete all '• Item' labels (undoable)")]
        public static void DeleteAll()
        {
#if UNITY_2022_2_OR_NEWER
            var all = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var all = Object.FindObjectsOfType<TMP_Text>(true);
#endif
            int del = 0;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            foreach (var tmp in all)
            {
                if (!tmp) continue;
                var s = (tmp.text ?? string.Empty).Trim();
                if (s == "• Item" || s == "Item")
                {
                    Undo.DestroyObjectImmediate(tmp.gameObject);
                    del++;
                }
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log(del == 0 ? "✅ Nothing to delete." : $"🧹 Deleted {del} placeholder object(s).");
        }

        private static string GetPath(Transform t)
        {
            if (t == null) return "(null)";
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            // If available, prefix with scene name for clarity
            var sceneName = t.gameObject.scene.IsValid() ? t.gameObject.scene.name : "(no-scene)";
            return $"{sceneName}/{path}";
        }
    }
}
#endif

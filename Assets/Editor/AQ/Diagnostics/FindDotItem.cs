#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace AQ.Editor.Diagnostics
{
    public static class FindDotItem
    {
        [MenuItem("AQ/Diag/Find all '• Item' labels")]
        public static void FindAll()
        {
            var all = Object.FindObjectsOfType<TMP_Text>(includeInactive:true);
            int hits = 0;
            foreach (var tmp in all)
            {
                var s = (tmp.text ?? "").Trim();
                if (s == "• Item" || s == "Item")
                {
                    hits++;
                    var path = GetPath(tmp.transform);
                    Debug.Log($"🔎 Found '{s}' at: {path}", tmp);
                }
            }
            Debug.Log(hits == 0 ? "✅ No '• Item' labels in scene." : $"🧭 Total matches: {hits}");
        }

        [MenuItem("AQ/Diag/Delete all '• Item' labels (undoable)")]
        public static void DeleteAll()
        {
            var all = Object.FindObjectsOfType<TMP_Text>(includeInactive:true);
            int del = 0;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            foreach (var tmp in all)
            {
                var s = (tmp.text ?? "").Trim();
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
            var path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace AQ.Editor.Requirements
{
    public static class FixTMPWrapping
    {
        [MenuItem("AQ/Requirements/Fix TMP Wrapping (NoWrap)")]
        public static void Run()
        {
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

#if UNITY_2022_2_OR_NEWER
            var tmps = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var tmps = Object.FindObjectsOfType<TMP_Text>(true);
#endif
            int changed = 0;
            foreach (var t in tmps)
            {
                if (!t) continue;

                // Optional: only fix under RequirementsHUD
                // if (!IsUnder(t.transform, "RequirementsHUD")) continue;

#if UNITY_2022_2_OR_NEWER
                if (t.textWrappingMode != TextWrappingModes.NoWrap)
                {
                    Undo.RecordObject(t, "Fix TMP Wrapping");
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    changed++;
                }
#else
                if (t.enableWordWrapping) // older API
                {
                    Undo.RecordObject(t, "Fix TMP Wrapping");
                    t.enableWordWrapping = false;
                    changed++;
                }
#endif
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log(changed == 0 ? "✅ All TMP labels already set to NoWrap." : $"🛠️ Fixed {changed} TMP label(s) to NoWrap.");
        }

        private static bool IsUnder(Transform t, string ancestorName)
        {
            while (t != null)
            {
                if (t.name == ancestorName) return true;
                t = t.parent;
            }
            return false;
        }
    }
}
#endif

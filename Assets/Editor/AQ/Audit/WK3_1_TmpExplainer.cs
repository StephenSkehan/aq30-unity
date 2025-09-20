using System.Linq;             // <-- gives Any/Where/Select
using UnityEngine;             // <-- gives Resources
using UnityEditor;

namespace AQ.EditorTools.Audit
{
    public static class WK3_1_TmpExplainer
    {
        [MenuItem("AQ/Audit/Explain TMP Components")]
        public static void Explain()
        {
            var roots = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.name == "ResolutionRoot");

            foreach (var root in roots)
            {
                var comps = root.GetComponentsInChildren<Component>(true);

                Debug.Log($"[TMPExplainer] ResolutionRoot has {comps.Length} components.");
                foreach (var c in comps)
                {
                    if (c == null) continue;
                    Debug.Log($" - {c.GetType().FullName}");
                }

                var hasTMP = comps.Any(c => c != null && c.GetType().Name == "TextMeshProUGUI");
                Debug.Log($"Has TMP? {hasTMP}");
            }
        }
    }
}

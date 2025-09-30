#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    public static class InspectTierSetPopupInstances
    {
        [MenuItem("AQ/Diagnostics/Leads/Inspect TierSetPopup Instances (All)")]
        public static void Run()
        {
            // Find all GameObjects (including inactive) named exactly "TierSetPopup"
            var all = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.name == "TierSetPopup")
                .Where(go =>
                {
                    // Only scene objects (not prefab assets)
                    var s = go.scene;
                    return s.IsValid() && s.isLoaded;
                })
                .ToList();

            if (all.Count == 0)
            {
                Debug.Log("[AQ Inspect] No TierSetPopup instances found.");
                return;
            }

            Debug.Log($"[AQ Inspect] TierSetPopup instances found: {all.Count}");
            foreach (var go in all)
            {
                var path = GetPath(go.transform);
                var scene = go.scene.name;
                var active = go.activeInHierarchy;
                int missing = CountMissingScriptsRecursive(go);
                var comps = DescribeComponents(go);

                Debug.Log($"[AQ Inspect] Scene='{scene}' Active={active} Path='{path}' MissingScripts={missing}\n  Components: {comps}");
            }
        }

        private static string GetPath(Transform t)
        {
            var stack = new List<string>();
            while (t != null) { stack.Add(t.name); t = t.parent; }
            stack.Reverse();
            return string.Join("/", stack);
        }

        private static int CountMissingScriptsRecursive(GameObject go)
        {
            int count = 0;
            foreach (var c in go.GetComponents<Component>()) if (!c) count++;
            foreach (Transform child in go.transform)
                count += CountMissingScriptsRecursive(child.gameObject);
            return count;
        }

        private static string DescribeComponents(GameObject go)
        {
            var list = new List<string>();
            foreach (var c in go.GetComponents<Component>())
            {
                if (!c) { list.Add("(Missing Script)"); continue; }
                list.Add(c.GetType().FullName);
            }
            return string.Join(", ", list);
        }
    }
}
#endif

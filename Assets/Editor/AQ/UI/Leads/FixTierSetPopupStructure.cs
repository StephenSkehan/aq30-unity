#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <- needed for SceneManager.GetActiveScene()

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Surgical fixer for TierSetPopup structure.
    /// - Targets only scene objects named "TierSetPopup".
    /// - Ensures an "IconGrid" child exists as a RectTransform with GridLayoutGroup,
    ///   creates Icon1..Icon6 if none exist, and ensures a "Highlight" Image exists (disabled).
    /// - Leaves everything else untouched (no presenter wiring, no card edits, no prefab churn).
    /// </summary>
    public static class FixTierSetPopupStructure
    {
        [MenuItem("AQ/UI/Leads/Fix TierSetPopup Structure (Icons/Grid Only)")]
        public static void Run()
        {
            var all = GameObject
                .FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name == "TierSetPopup")
                .Select(t => t.gameObject)
                .ToArray();

            if (all.Length == 0)
            {
                Debug.LogWarning("[AQ FixPopup] No TierSetPopup in scene.");
                return;
            }

            foreach (var go in all)
            {
                var t = go.transform;

                // 1) Ensure IconGrid exists and is a RectTransform
                var gridTr = t.Find("IconGrid");
                if (gridTr && !(gridTr is RectTransform))
                {
                    // Preserve any legacy Transform; rename it and create a proper UI grid.
                    gridTr.gameObject.name = "IconGrid (legacy)";
                    gridTr = null;
                }

                if (!gridTr)
                {
                    var gridGO = new GameObject("IconGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                    gridTr = gridGO.transform;
                    gridTr.SetParent(t, false);

                    var rt = (RectTransform)gridTr;
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(480, 120);
                    rt.anchoredPosition = new Vector2(0, 10);

                    var layout = gridGO.GetComponent<GridLayoutGroup>();
                    layout.cellSize = new Vector2(64, 64);
                    layout.spacing = new Vector2(12, 12);
                    layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    layout.constraintCount = 1;
                }
                else
                {
                    // Ensure a GridLayoutGroup exists with sane defaults.
                    var layout = gridTr.GetComponent<GridLayoutGroup>() ?? gridTr.gameObject.AddComponent<GridLayoutGroup>();
                    if (layout.cellSize == Vector2.zero) layout.cellSize = new Vector2(64, 64);
                }

                // 2) Ensure Icons exist (only create if none present)
                int existingIcons = gridTr.Cast<Transform>().Count(ch => ch.name.StartsWith("Icon"));
                if (existingIcons == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        var iconTf = new GameObject($"Icon{i + 1}", typeof(RectTransform), typeof(Image)).transform;
                        iconTf.SetParent(gridTr, false);
                        ((RectTransform)iconTf).sizeDelta = new Vector2(64, 64);
                        var img = iconTf.GetComponent<Image>();
                        img.raycastTarget = false;
                        img.enabled = true; // visible when a sprite is assigned by the runtime presenter
                    }
                }

                // 3) Ensure Highlight exists
                if (!gridTr.Find("Highlight"))
                {
                    var hlTf = new GameObject("Highlight", typeof(RectTransform), typeof(Image)).transform;
                    hlTf.SetParent(gridTr, false);
                    ((RectTransform)hlTf).sizeDelta = new Vector2(72, 72);
                    var img = hlTf.GetComponent<Image>();
                    img.raycastTarget = false;
                    img.enabled = false; // presenter toggles visibility
                }

                Debug.Log($"[AQ FixPopup] Repaired IconGrid on '{PathOf(go.transform)}'.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static string PathOf(Transform t)
        {
            System.Collections.Generic.List<string> parts = new();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif

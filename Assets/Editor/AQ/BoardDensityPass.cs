using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// Board density pass (GH reference): separates the continuous checker grid
    /// into discrete tile plates with small gutters, and insets each slot's
    /// Item image so icons fill ~82% of the cell instead of full-bleed.
    /// Idempotent — safe to re-run. Slots are found by slot_RR_CC naming
    /// (their types live in Assembly-CSharp, which this assembly cannot
    /// reference).
    /// </summary>
    public static class BoardDensityPass
    {
        const float CellSize = 142f;
        const float Gutter = 6f;
        static readonly Vector2 ItemAnchorMin = new Vector2(0.09f, 0.09f);
        static readonly Vector2 ItemAnchorMax = new Vector2(0.91f, 0.91f);

        [MenuItem("AQ/Setup/Board Density Pass")]
        public static void Apply()
        {
            var slotRe = new Regex(@"^slot_(\d{2})_(\d{2})$");
            GridLayoutGroup grid = null;
            int items = 0;

            foreach (var rt in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!slotRe.IsMatch(rt.name)) continue;

                if (grid == null && rt.parent != null)
                    grid = rt.parent.GetComponent<GridLayoutGroup>();

                var item = rt.Find("Item") as RectTransform;
                if (item == null) continue;

                Undo.RecordObject(item, "Board density");
                item.anchorMin = ItemAnchorMin;
                item.anchorMax = ItemAnchorMax;
                item.offsetMin = Vector2.zero;
                item.offsetMax = Vector2.zero;
                EditorUtility.SetDirty(item);
                items++;
            }

            if (grid == null)
            {
                Debug.LogError("[Board] no slot_RR_CC grid found in the open scene.");
                return;
            }

            Undo.RecordObject(grid, "Board density");
            grid.cellSize = new Vector2(CellSize, CellSize);
            grid.spacing = new Vector2(Gutter, Gutter);
            EditorUtility.SetDirty(grid);

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[Board] density pass: cell {CellSize}px, gutter {Gutter}px, {items} item images inset to ~82% fill.");
        }
    }
}

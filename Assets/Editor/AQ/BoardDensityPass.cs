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
    /// The GridLayoutGroup is OWNED by MergeBoardPresenter ([ExecuteAlways],
    /// recomputes cellSize/spacing from its serialized Spacing) and re-asserted
    /// by MergeBoardBoot at runtime — so the gutter must be written into BOTH
    /// components' serialized fields, never onto the grid directly (a direct
    /// write is silently stomped on the next layout pass; cost a regression).
    /// Idempotent — safe to re-run. Types live in Assembly-CSharp, which this
    /// assembly cannot reference, so fields are set via SerializedObject.
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
            Transform boardRoot = null;
            int items = 0;

            foreach (var rt in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!slotRe.IsMatch(rt.name)) continue;

                if (boardRoot == null) boardRoot = rt.parent;

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

            if (boardRoot == null)
            {
                Debug.LogError("[Board] no slot_RR_CC grid found in the open scene.");
                return;
            }

            bool presenterSet = SetSerialized(boardRoot, "MergeBoardPresenter", "Spacing", Gutter);
            bool bootSet = SetSerializedVector2(boardRoot, "MergeBoardBoot", "spacing", new Vector2(Gutter, Gutter));
            if (!presenterSet)
                Debug.LogWarning("[Board] MergeBoardPresenter.Spacing not set.");
            if (!bootSet)
                Debug.LogWarning("[Board] MergeBoardBoot.spacing not set — gutter will be stomped at runtime.");

            var grid = boardRoot.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Undo.RecordObject(grid, "Board density");
                grid.cellSize = new Vector2(CellSize, CellSize);
                grid.spacing = new Vector2(Gutter, Gutter);
                EditorUtility.SetDirty(grid);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[Board] density pass: cell {CellSize}px, gutter {Gutter}px (grid + presenter + boot), {items} item images inset to ~82% fill.");
        }

        static bool SetSerialized(Transform root, string componentType, string field, float value)
        {
            var comp = root.GetComponent(componentType);
            if (comp == null) return false;
            var so = new SerializedObject(comp);
            var prop = so.FindProperty(field);
            if (prop == null) return false;
            prop.floatValue = value;
            so.ApplyModifiedProperties();
            return true;
        }

        static bool SetSerializedVector2(Transform root, string componentType, string field, Vector2 value)
        {
            var comp = root.GetComponent(componentType);
            if (comp == null) return false;
            var so = new SerializedObject(comp);
            var prop = so.FindProperty(field);
            if (prop == null) return false;
            prop.vector2Value = value;
            so.ApplyModifiedProperties();
            return true;
        }
    }
}

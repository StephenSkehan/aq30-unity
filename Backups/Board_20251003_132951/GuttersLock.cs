// Assets/Scripts/UI/Board/GuttersLock.cs
//
// Purpose
//   Enforce 2px black gutters and full-bleed icons for the merge board
//   in both Edit and Play, even if other code rebuilds the grid.
//
// How to use
//   1) Add this component to the MergeBoard GameObject (same object that
//      holds your GridLayoutGroup / MergeBoardController).
//   2) Optionally assign Board Root (RectTransform) and Grid (GridLayoutGroup)
//      in the Inspector; otherwise they are auto-found.
//   3) Leave it there. It keeps the layout correct in Edit & Play.

using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    [ExecuteAlways]
    public sealed class GuttersLock : MonoBehaviour
    {
        [Header("Targets (auto-found if left empty)")]
        public RectTransform boardRoot;
        public GridLayoutGroup grid;

        [Header("Style")]
        [Tooltip("Gutter thickness in pixels (2 by default).")]
        [Range(0, 8)] public int gutterPx = 2;
        [Tooltip("Board background color. Gutters show as this color.")]
        public Color gutterColor = Color.black;

        // Run once after first render in Play Mode (ensures grid children exist)
        bool _appliedThisPlay;

        void Reset() => AutoFind();

        void OnEnable()
        {
            AutoFind();
            // Always apply in Edit Mode so the scene view shows correct layout
#if UNITY_EDITOR
            if (!Application.isPlaying) Apply();
#endif

            // In Play, apply the first frame after grid is built.
            Canvas.willRenderCanvases -= OnFirstRenderThenApply;
            Canvas.willRenderCanvases += OnFirstRenderThenApply;
        }

        void OnDisable()
        {
            Canvas.willRenderCanvases -= OnFirstRenderThenApply;
            _appliedThisPlay = false;
        }

#if UNITY_EDITOR
        // Keep layout correct if someone edits the prefab/scene while not playing.
        void Update()
        {
            if (!Application.isPlaying)
                Apply();
        }
#endif

        void OnFirstRenderThenApply()
        {
            if (!Application.isPlaying || _appliedThisPlay)
                return;

            // Wait until grid exists and has at least some children (slots)
            AutoFind();
            if (!grid || grid.transform.childCount == 0)
                return;

            Apply();
            _appliedThisPlay = true;
        }

        void AutoFind()
        {
            if (!grid)
                grid = GetComponentInChildren<GridLayoutGroup>(true);
            if (!boardRoot && grid)
                boardRoot = grid.GetComponent<RectTransform>();
        }

        /// <summary>Apply visual rules to board + every slot.</summary>
        public void Apply()
        {
            if (!grid) return;

            // 1) Grid: 2px spacing (gutters), consistent alignment
            var want = new Vector2(gutterPx, gutterPx);
            if (grid.spacing != want) grid.spacing = want;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis   = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            // 2) Board background = black (so the gutters are visible)
            if (boardRoot)
            {
                var bg = boardRoot.GetComponent<Image>() ?? boardRoot.gameObject.AddComponent<Image>();
                bg.sprite = null;
                bg.type = Image.Type.Simple;
                bg.color = gutterColor;
                bg.raycastTarget = false;
            }

            // 3) Normalize every slot to full-bleed with 2px inset
            var inset = gutterPx;
            for (int i = 0; i < grid.transform.childCount; i++)
            {
                if (!(grid.transform.GetChild(i) is RectTransform slot))
                    continue;

                NormalizeSlot(slot, inset);
            }
        }

        static void StretchToCell(RectTransform rt, int insetPx)
        {
            if (!rt) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.one * insetPx;
            rt.offsetMax = -Vector2.one * insetPx;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        static void ForceSimple(Image img, bool preserveAspect)
        {
            if (!img) return;
            img.type = Image.Type.Simple;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = true;
            // color left as-is for content images
        }

        /// <summary>
        /// Bring a slot prefab instance into compliance:
        /// - Bg fills cell with inset (for the border line)
        /// - Icon fills the same area (no extra padding)
        /// - Remove/neutralize layout components that cause shrinking
        /// </summary>
        static void NormalizeSlot(RectTransform slot, int insetPx)
        {
            if (!slot) return;

            // Common children names we’ve seen in this project
            var bg   = slot.Find("Bg")   as RectTransform;
            var icon = (slot.Find("Icon") ?? slot.Find("Item")) as RectTransform;

            if (bg)
            {
                StretchToCell(bg, insetPx);
                ForceSimple(bg.GetComponent<Image>(), preserveAspect: false);
                // Bg usually white; leave color unchanged to match existing art
            }

            if (icon)
            {
                StretchToCell(icon, insetPx);
                ForceSimple(icon.GetComponent<Image>(), preserveAspect: false);

#if UNITY_EDITOR
                // Kill anything that enforces a different aspect/size
                var arf = icon.GetComponent<AspectRatioFitter>();
                if (arf) UnityEditor.Undo.DestroyObjectImmediate(arf);

                var le = icon.GetComponent<LayoutElement>();
                if (le)
                {
                    le.minWidth = le.minHeight = -1;
                    le.preferredWidth = le.preferredHeight = -1;
                }
#endif
            }
        }
    }
}

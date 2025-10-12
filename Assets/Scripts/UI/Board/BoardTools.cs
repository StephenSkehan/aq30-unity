using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Runtime helpers for the merge board. This version is intentionally
    /// self-contained and does NOT rely on MergeBoardController exposing
    /// BoardRoot / DragLayer properties (so it compiles against your current code).
    /// </summary>
    public static class BoardTools
    {
        // ─────────────────────────────────────────────────────────────────────────
        // Root & Grid lookups (no controller properties required)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Returns the RectTransform that acts as the board root.</summary>
        public static RectTransform GetBoardRoot(this MergeBoardController ctrl)
        {
            return ctrl ? ctrl.transform as RectTransform : null;
        }

        /// <summary>Returns the GridLayoutGroup on the controller object.</summary>
        public static GridLayoutGroup GetGrid(this MergeBoardController ctrl)
        {
            return ctrl ? ctrl.GetComponent<GridLayoutGroup>() : null;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Drag layer / drag ghost
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ensures there's a full-screen DragLayer (non-blocking) under the board root.
        /// Returns its RectTransform.
        /// </summary>
        public static RectTransform EnsureDragLayer(this MergeBoardController ctrl)
        {
            var root = ctrl.GetBoardRoot();
            if (!root) return null;

            var t = root.Find("DragLayer");
            if (!t)
            {
                var go = new GameObject("DragLayer", typeof(RectTransform));
                t = go.transform;
                t.SetParent(root, worldPositionStays: false);
            }

            var rt = (RectTransform)t;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot    = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            // Make sure this sits on top but does not block pointer events
            var cg = rt.GetComponent<CanvasGroup>();
            if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
            cg.ignoreParentGroups = false;

            rt.SetAsLastSibling();
            return rt;
        }

        /// <summary>
        /// Ensures a DragGhost Image exists under the DragLayer and returns it.
        /// </summary>
        public static Image EnsureDragGhost(this MergeBoardController ctrl)
        {
            var dragLayer = ctrl.EnsureDragLayer();
            if (!dragLayer) return null;

            var ghostTr = dragLayer.Find("DragGhost");
            if (!ghostTr)
            {
                var go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
                ghostTr = go.transform;
                ghostTr.SetParent(dragLayer, worldPositionStays: false);
            }

            var ghost = ghostTr.GetComponent<Image>();
            ghost.raycastTarget = false;       // never block input
            ghost.enabled = false;             // hidden until used
            return ghost;
        }

        /// <summary>Show/update the drag ghost image and size.</summary>
        public static void ShowDragGhost(this MergeBoardController ctrl, Sprite sprite, Vector2 size)
        {
            var ghost = ctrl.EnsureDragGhost();
            if (!ghost) return;

            ghost.sprite = sprite;
            ghost.preserveAspect = true;

            var rt = (RectTransform)ghost.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;               // keep same visual size as cell
            ghost.enabled = sprite != null;
        }

        /// <summary>Hide the drag ghost.</summary>
        public static void HideDragGhost(this MergeBoardController ctrl)
        {
            var ghost = ctrl.EnsureDragGhost();
            if (ghost) ghost.enabled = false;
        }

        /// <summary>
        /// Position the drag ghost using a screen-space pointer position.
        /// </summary>
        public static void PositionDragGhostFromScreen(this MergeBoardController ctrl, Vector2 screenPos)
        {
            var ghost = ctrl.EnsureDragGhost();
            var root  = ctrl.GetBoardRoot();
            if (!ghost || !root) return;

            var rt = (RectTransform)ghost.transform;

            // Use the board canvas for conversion
            var canvas = root.GetComponentInParent<Canvas>();
            if (!canvas) return;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    root, screenPos, null, out var local);
                rt.anchoredPosition = local;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    root, screenPos, canvas.worldCamera, out var local);
                rt.anchoredPosition = local;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Index helpers (stable & safe)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a linear slot index to row/col using the controller's Rows/Cols.
        /// </summary>
        public static (int row, int col) IndexToRowCol(this MergeBoardController ctrl, int index)
        {
            int cols = Mathf.Max(1, ctrl.Cols);
            int row  = index / cols;
            int col  = index % cols;
            return (row, col);
        }

        /// <summary>
        /// Converts row/col to linear index using the controller's Rows/Cols.
        /// </summary>
        public static int RowColToIndex(this MergeBoardController ctrl, int row, int col)
        {
            return row * Mathf.Max(1, ctrl.Cols) + col;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // (Deprecated) – left commented intentionally to avoid tech debt confusion.
        // ─────────────────────────────────────────────────────────────────────────

        /*
        // OLD: Relied on a MergeBoardController.BoardRoot property that your current
        // controller does not expose yet. Keeping it here (commented) as a breadcrumb.
        public static RectTransform BoardRoot(this MergeBoardController ctrl) => ctrl.BoardRoot;

        // OLD: Relied on a MergeBoardController.Grid property. We now call
        // GetComponent<GridLayoutGroup>() directly for resilience.
        public static GridLayoutGroup Grid(this MergeBoardController ctrl) => ctrl.Grid;
        */
    }
}

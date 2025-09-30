using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AQ.UI
{
    /// <summary>
    /// Lays out the merge board grid under the HUD lead cards.
    /// - Stretches to full width with side margins
    /// - Positions top edge just below LeadsBar
    /// - Forces a 7x9 (or configured) grid of square cells that always fit
    /// </summary>
    [ExecuteAlways]
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Structure")]
        public RectTransform BoardFrame;     // container that stretches with margins
        public RectTransform BoardViewport;  // inner rect that the grid fills
        public RectTransform Grid;           // GameObject that holds the tile children

        [Header("HUD anchors")]
        public RectTransform LeadsBar;       // used to compute the top inset

        [Header("Grid Settings")]
        [Min(1)] public int Columns = 7;
        [Min(1)] public int Rows = 9;
        [Min(0)] public float Spacing = 12f;
        [Min(0)] public float SideMargin = 24f;
        [Min(0)] public float BottomMargin = 48f;
        [Min(0)] public float ExtraTopGap = 8f; // breathing room under the lead cards

        private GridLayoutGroup _layout;

        private void Awake()
        {
            Cache();
        }

        private void OnEnable()
        {
            Cache();
            ApplyLayout();
        }

        private void OnTransformChildrenChanged()
        {
            Cache();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyLayout();
        }

        private void Cache()
        {
            if (Grid == null) return;
            _layout = Grid.GetComponent<GridLayoutGroup>();
            if (_layout == null) _layout = Grid.gameObject.AddComponent<GridLayoutGroup>();
            ConfigureLayoutGroup();
        }

        private void ConfigureLayoutGroup()
        {
            if (_layout == null) return;
            _layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _layout.constraintCount = Mathf.Max(1, Columns);
            _layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            _layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            _layout.childAlignment = TextAnchor.UpperCenter;
            _layout.spacing = new Vector2(Spacing, Spacing);
        }

        [ContextMenu("Apply Layout")]
        public void ApplyLayout()
        {
            if (BoardFrame == null || BoardViewport == null || Grid == null) return;

            // Stretch the frame to canvas with margins
            BoardFrame.anchorMin = new Vector2(0f, 0f);
            BoardFrame.anchorMax = new Vector2(1f, 1f);
            BoardFrame.pivot = new Vector2(0.5f, 1f);

            var canvas = GetComponentInParent<Canvas>();
            var canvasRT = canvas ? canvas.transform as RectTransform : transform as RectTransform;

            float topInset = 0f;
            if (LeadsBar != null && canvasRT != null)
            {
                // Bounds of LeadsBar relative to canvas
                var b = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRT, LeadsBar);
                float canvasTop = canvasRT.rect.height * 0.5f; // canvas local top Y
                float leadsBottom = b.min.y;                   // bottom of LeadsBar in canvas local
                topInset = (canvasTop - leadsBottom) + ExtraTopGap;
            }

            var min = BoardFrame.offsetMin; // left, bottom
            var max = BoardFrame.offsetMax; // right, top (negative)
            min.x = SideMargin;
            min.y = BottomMargin;
            max.x = -SideMargin;
            max.y = -topInset;
            BoardFrame.offsetMin = min;
            BoardFrame.offsetMax = max;

            // Viewport fills the frame
            BoardViewport.anchorMin = Vector2.zero;
            BoardViewport.anchorMax = Vector2.one;
            BoardViewport.pivot = new Vector2(0.5f, 1f);
            BoardViewport.offsetMin = Vector2.zero;
            BoardViewport.offsetMax = Vector2.zero;

            // Compute square cell size that fits both width and height
            ConfigureLayoutGroup();
#if UNITY_EDITOR
            if (!Application.isPlaying && BoardViewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(BoardViewport);
#endif
            float availW = Mathf.Max(0f, BoardViewport.rect.width);
            float availH = Mathf.Max(0f, BoardViewport.rect.height);

            float cellW = (availW - Spacing * (Columns - 1)) / Columns;
            float cellH = (availH - Spacing * (Rows - 1)) / Rows;
            float cell = Mathf.Floor(Mathf.Min(cellW, cellH));

            if (_layout != null)
            {
                _layout.cellSize = new Vector2(cell, cell);
                _layout.spacing = new Vector2(Spacing, Spacing);
                _layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _layout.constraintCount = Columns;
            }
        }
    }
}

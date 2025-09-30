using UnityEngine;
using UnityEngine.UI;

namespace AQ.UI
{
    /// <summary>
    /// Deterministic 7x9 board layout. Fills width (respecting side margins),
    /// clamps height to fit rows, and honors a fixed TopInset set by the editor tool.
    /// Also applies a uniform CellColor to every child Image in the grid.
    /// </summary>
    [ExecuteAlways]
    public class MergeBoardPresenter : MonoBehaviour
    {
        [Header("Structure (assigned by builder)")]
        public RectTransform BoardFrame;     // stretches across canvas with margins
        public RectTransform BoardViewport;  // the area the grid fills
        public RectTransform Grid;           // has GridLayoutGroup

        [Header("Grid")]
        [Min(1)] public int Columns = 7;
        [Min(1)] public int Rows    = 9;
        [Min(0)] public float Spacing = 2f;  // requested 2px spacing

        [Header("Margins")]
        [Min(0)] public float SideMargin   = 24f;
        [Min(0)] public float BottomMargin = 48f;
        [Min(0)] public float TopInset     = 660f; // set by builder

        [Header("Visual")]
        public Color CellColor = Color.white;

        private GridLayoutGroup _layout;

        void Awake()                            => Cache();
        void OnEnable()                         { Cache(); ApplyLayout(); ApplyAppearance(); }
        void OnValidate()                       { Cache(); ApplyLayout(); ApplyAppearance(); }
        void OnRectTransformDimensionsChange()  { ApplyLayout(); }

        private void Cache()
        {
            if (Grid == null) return;
            if (_layout == null) _layout = Grid.GetComponent<GridLayoutGroup>();
            if (_layout == null) _layout = Grid.gameObject.AddComponent<GridLayoutGroup>();

            _layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            _layout.constraintCount = Mathf.Max(1, Columns);
            _layout.startAxis       = GridLayoutGroup.Axis.Horizontal;
            _layout.startCorner     = GridLayoutGroup.Corner.UpperLeft;
            _layout.childAlignment  = TextAnchor.UpperCenter;
            _layout.spacing         = new Vector2(Spacing, Spacing);
        }

        [ContextMenu("Apply Layout")]
        public void ApplyLayout()
        {
            if (BoardFrame == null || BoardViewport == null || Grid == null) return;

            // Stretch the frame to canvas with margins and top inset.
            BoardFrame.anchorMin = new Vector2(0f, 0f);
            BoardFrame.anchorMax = new Vector2(1f, 1f);
            BoardFrame.pivot     = new Vector2(0.5f, 1f);
            BoardFrame.offsetMin = new Vector2(SideMargin, BottomMargin);
            BoardFrame.offsetMax = new Vector2(-SideMargin, -TopInset);

            // Viewport fills the frame.
            BoardViewport.anchorMin = Vector2.zero;
            BoardViewport.anchorMax = Vector2.one;
            BoardViewport.pivot     = new Vector2(0.5f, 1f);
            BoardViewport.offsetMin = Vector2.zero;
            BoardViewport.offsetMax = Vector2.zero;

            if (_layout == null) Cache();

            // Compute square cell that fills width and fits height.
            float availW = Mathf.Max(0f, BoardViewport.rect.width);
            float availH = Mathf.Max(0f, BoardViewport.rect.height);

            float cellW = (availW - Spacing * (Columns - 1)) / Columns;
            float cellH = (availH - Spacing * (Rows    - 1)) / Rows;
            float cell  = Mathf.Floor(Mathf.Min(cellW, cellH));

            _layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            _layout.constraintCount = Columns;
            _layout.spacing         = new Vector2(Spacing, Spacing);
            _layout.cellSize        = new Vector2(cell, cell);
        }

        [ContextMenu("Apply Appearance")]
        public void ApplyAppearance()
        {
            if (Grid == null) return;
            for (int i = 0; i < Grid.childCount; i++)
            {
                var t = Grid.GetChild(i);
                var img = t.GetComponent<Image>();
                if (img != null)
                {
                    img.color = CellColor;               // pure white (or whatever is set)
                    img.raycastTarget = false;
                    // img.sprite remains null so it renders as a solid quad with CellColor.
                }
            }
        }
    }
}

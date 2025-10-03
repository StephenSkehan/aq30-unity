using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class MergeBoardPresenter : MonoBehaviour
    {
        const string BgName = "__AQ_BoardBG";

        void OnEnable()
        {
            var grid = GetComponent<GridLayoutGroup>();
            if (grid)
            {
                grid.spacing = new Vector2(2, 2);
                // The controller will set FixedColumnCount = Cols; keep Flexible here in edit to avoid surprises.
                if (grid.constraint == GridLayoutGroup.Constraint.Flexible)
                    grid.constraint = GridLayoutGroup.Constraint.Flexible;
            }

            var rt = (RectTransform)transform;
            // Ensure black background child exists and is stretched
            var bg = transform.Find(BgName) as RectTransform;
            if (!bg)
            {
                var go = new GameObject(BgName, typeof(RectTransform), typeof(Image));
                bg = go.GetComponent<RectTransform>();
                bg.SetParent(transform, false);
                bg.SetAsFirstSibling();
            }
            var img = bg.GetComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;

            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
        }
    }
}

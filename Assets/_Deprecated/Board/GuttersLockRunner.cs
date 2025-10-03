using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    // Add this to the same GameObject that has MergeBoardPresenter.
    public sealed class GuttersLockRunner : MonoBehaviour
    {
        GridLayoutGroup grid;
        RectTransform bg;

        void Awake()
        {
            grid = GetComponent<GridLayoutGroup>();
            var t = transform.Find("__AQ_BoardBG");
            bg = t ? (RectTransform)t : null;
        }

        void LateUpdate()
        {
            if (grid)
            {
                if (grid.spacing != new Vector2(2, 2))
                    grid.spacing = new Vector2(2, 2);
            }
            if (!bg)
            {
                var go = new GameObject("__AQ_BoardBG", typeof(RectTransform), typeof(Image));
                bg = go.GetComponent<RectTransform>();
                bg.SetParent(transform, false);
                bg.SetAsFirstSibling();
            }
            var img = bg.GetComponent<Image>();
            if (img && img.color != Color.black)
                img.color = Color.black;

            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
        }
    }
}

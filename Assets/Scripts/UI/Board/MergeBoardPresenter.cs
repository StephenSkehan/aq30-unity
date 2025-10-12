using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Sets up the merge board visuals once: 2px gutters and black background.
    /// No constant enforcement - just set and forget.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class MergeBoardPresenter : MonoBehaviour
    {
        const string BgName = "__AQ_BoardBG";

        void Awake()
        {
            // Set spacing ONCE in Awake
            var grid = GetComponent<GridLayoutGroup>();
            if (grid && grid.spacing != new Vector2(2, 2))
            {
                grid.spacing = new Vector2(2, 2);
                Debug.Log("[MergeBoardPresenter] Set grid spacing to (2, 2)");
            }

            // Ensure black background exists
            EnsureBackground();
        }

        void EnsureBackground()
        {
            var bg = transform.Find(BgName) as RectTransform;
            if (!bg)
            {
                var go = new GameObject(BgName, typeof(RectTransform), typeof(Image));
                bg = go.GetComponent<RectTransform>();
                bg.SetParent(transform, false);
                bg.SetAsFirstSibling();
                
                var img = bg.GetComponent<Image>();
                img.color = Color.black;
                img.raycastTarget = false;
                
                bg.anchorMin = Vector2.zero;
                bg.anchorMax = Vector2.one;
                bg.offsetMin = Vector2.zero;
                bg.offsetMax = Vector2.zero;
                
                Debug.Log("[MergeBoardPresenter] Created black background");
            }
        }
    }
}
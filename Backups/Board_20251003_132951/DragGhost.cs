using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Lightweight drag visual that follows the cursor while dragging.
    /// Created lazily; parented under the board Canvas.
    /// </summary>
    public sealed class DragGhost : MonoBehaviour
    {
        private RectTransform _rt;
        private CanvasGroup _cg;
        private Image _img;

        public static DragGhost Create(Transform parentCanvas)
        {
            var go = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            go.layer = parentCanvas.gameObject.layer;
            go.transform.SetParent(parentCanvas, false);
            var g = go.AddComponent<DragGhost>();
            g.Init();
            g.Hide();
            return g;
        }

        private void Init()
        {
            _rt = GetComponent<RectTransform>();
            _cg = GetComponent<CanvasGroup>();
            _img = GetComponent<Image>();
            _img.raycastTarget = false;
            _img.preserveAspect = true;
        }

        public void Show(Sprite sprite, RectTransform sourceRect)
        {
            if (_img == null) Init();
            _img.sprite = sprite;
            _img.enabled = sprite != null;
            // size to source cell
            _rt.anchorMin = Vector2.zero;
            _rt.anchorMax = Vector2.zero;
            _rt.sizeDelta = sourceRect.rect.size;
            _cg.alpha = 0.85f;
            gameObject.SetActive(true);
        }

        public void MoveToScreen(Vector2 screenPos, Canvas canvas)
        {
            if (_rt == null || canvas == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform, screenPos, null, out var local);
            _rt.anchoredPosition = local;
        }

        public void Hide()
        {
            if (!this) return;
            if (_cg == null) _cg = GetComponent<CanvasGroup>();
            if (_img == null) _img = GetComponent<Image>();
            if (_img != null) _img.enabled = false;
            if (_cg != null) _cg.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}

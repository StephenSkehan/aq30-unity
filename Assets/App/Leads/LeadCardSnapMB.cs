using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    /// <summary>
    /// Snap-paging for the leads bar: when a horizontal drag ends, the content
    /// eases to the nearest card boundary (biased by fling direction) instead
    /// of drifting to an arbitrary offset. Added at runtime by LeadsBarView.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class LeadCardSnapMB : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        const float SnapSeconds = 0.16f;
        const float FlingThreshold = 350f;

        ScrollRect _scroll;
        Coroutine _snap;

        void Awake()
        {
            _scroll = GetComponent<ScrollRect>();
            _scroll.inertia = false; // paging feel: crisp stop + snap, no drift
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_snap != null) StopCoroutine(_snap);
            _snap = null;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var content = _scroll.content;
            if (content == null || content.childCount == 0) return;

            float step = SnapStep(content);
            if (step <= 1f) return;

            float x = -content.anchoredPosition.x;
            float idx = x / step;

            float vx = eventData.delta.x / Mathf.Max(Time.unscaledDeltaTime, 0.001f);
            if (Mathf.Abs(vx) > FlingThreshold)
                idx = vx < 0 ? Mathf.Ceil(idx) : Mathf.Floor(idx);
            else
                idx = Mathf.Round(idx);

            float maxOffset = Mathf.Max(0f, content.rect.width - ((RectTransform)_scroll.transform).rect.width);
            float target = Mathf.Clamp(idx * step, 0f, maxOffset);

            if (_snap != null) StopCoroutine(_snap);
            _snap = StartCoroutine(SnapTo(content, -target));
        }

        static float SnapStep(RectTransform content)
        {
            var first = content.GetChild(0) as RectTransform;
            if (first == null) return 0f;
            float spacing = 0f;
            var layout = content.GetComponent<HorizontalLayoutGroup>();
            if (layout != null) spacing = layout.spacing;
            return first.rect.width + spacing;
        }

        IEnumerator SnapTo(RectTransform content, float targetX)
        {
            float startX = content.anchoredPosition.x;
            float t = 0f;
            while (t < SnapSeconds)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / SnapSeconds);
                k = k * k * (3f - 2f * k);
                content.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, k), content.anchoredPosition.y);
                yield return null;
            }
            content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
            _snap = null;
        }
    }
}

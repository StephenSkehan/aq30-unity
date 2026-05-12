using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    public static class StringConnectionLine
    {
        public static void Create(RectTransform parent, RectTransform from, RectTransform to)
        {
            var fromPos = from.anchoredPosition;
            var toPos   = to.anchoredPosition;
            var center  = (fromPos + toPos) * 0.5f;
            float dist  = Vector2.Distance(fromPos, toPos);
            float angle = Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg;

            var go = new GameObject("String", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt             = go.GetComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 0.5f);
            rt.anchorMax       = new Vector2(0.5f, 0.5f);
            rt.pivot           = new Vector2(0.5f, 0.5f);
            rt.sizeDelta       = new Vector2(dist, 5f);
            rt.anchoredPosition = center;
            rt.localRotation   = Quaternion.Euler(0f, 0f, angle);

            go.GetComponent<Image>().color = new Color(0.82f, 0.12f, 0.12f, 0.85f);
            rt.SetAsFirstSibling();
        }
    }
}

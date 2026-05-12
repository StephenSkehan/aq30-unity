using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    public static class EvidenceItemPin
    {
        private static readonly Color CardColor = new Color(0.97f, 0.96f, 0.88f, 1f);
        private static readonly Color TackColor = new Color(0.20f, 0.50f, 0.90f, 1f);

        public static RectTransform Create(RectTransform parent, string label, Sprite icon, Vector2 pos, Sprite tackSprite = null)
        {
            var card             = MakeRect("EvidPin_" + label, parent);
            card.anchorMin       = new Vector2(0.5f, 0.5f);
            card.anchorMax       = new Vector2(0.5f, 0.5f);
            card.pivot           = new Vector2(0.5f, 0.5f);
            card.sizeDelta       = new Vector2(220f, 220f);
            card.anchoredPosition = pos;
            card.localRotation   = Quaternion.Euler(0f, 0f, Tilt(label));
            card.gameObject.AddComponent<Image>().color = CardColor;

            AddTack("Pin", card, new Vector2(0f, 88f), 40f, tackSprite);

            if (icon != null)
            {
                var iconRt              = MakeRect("Icon", card);
                iconRt.anchorMin        = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot            = new Vector2(0.5f, 0.5f);
                iconRt.sizeDelta        = new Vector2(120f, 120f);
                iconRt.anchoredPosition = new Vector2(0f, 20f);
                var img                 = iconRt.gameObject.AddComponent<Image>();
                img.sprite              = icon;
                img.preserveAspect      = true;
                img.raycastTarget       = false;
            }

            var lblRt              = MakeRect("Label", card);
            lblRt.anchorMin        = new Vector2(0f, 0f);
            lblRt.anchorMax        = new Vector2(1f, 0f);
            lblRt.pivot            = new Vector2(0.5f, 0f);
            lblRt.sizeDelta        = new Vector2(-10f, 50f);
            lblRt.anchoredPosition = new Vector2(0f, 10f);
            var tmp                = lblRt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text               = label;
            tmp.fontSize           = 22f;
            tmp.color              = new Color(0.15f, 0.10f, 0.05f, 1f);
            tmp.alignment          = TextAlignmentOptions.Center;
            tmp.raycastTarget      = false;

            return card;
        }

        private static void AddTack(string name, RectTransform parent, Vector2 pos, float size, Sprite sprite)
        {
            var rt              = MakeRect(name, parent);
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img             = rt.gameObject.AddComponent<Image>();
            if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
            else                { img.color  = TackColor; }
        }

        private static float Tilt(string seed)
        {
            var rng = new System.Random(seed.GetHashCode());
            return (float)(rng.NextDouble() * 10.0 - 5.0);
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }
    }
}

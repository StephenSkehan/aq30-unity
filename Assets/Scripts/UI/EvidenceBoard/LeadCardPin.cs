using System;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    /// <summary>Index card pinned to the evidence board. Taps arrive via the
    /// board's raw-input poll (EvidenceBoardScreen), not GraphicRaycaster —
    /// the board canvas is boot-created, where GR clicks are unreliable.</summary>
    [RequireComponent(typeof(Image))]
    public class LeadCardPin : MonoBehaviour
    {
        private LeadData _lead;
        private Action<LeadData> _onTap;

        public void Tap() => _onTap?.Invoke(_lead);

        public static RectTransform Create(RectTransform parent, LeadData lead, Vector2 pos, Action<LeadData> onTap, Sprite tackSprite = null)
        {
            var card              = MakeRect("Card_" + lead.leadId, parent);
            card.anchorMin        = new Vector2(0.5f, 0.5f);
            card.anchorMax        = new Vector2(0.5f, 0.5f);
            card.pivot            = new Vector2(0.5f, 0.5f);
            card.sizeDelta        = new Vector2(380f, 300f);
            card.anchoredPosition = pos;
            card.localRotation    = Quaternion.Euler(0f, 0f, Tilt(lead.leadId));
            card.gameObject.AddComponent<Image>().color = new Color(0.97f, 0.96f, 0.88f, 1f);

            var pin       = card.gameObject.AddComponent<LeadCardPin>();
            pin._lead     = lead;
            pin._onTap    = onTap;

            AddShadow(card);

            // Thumbtack
            AddTack("Tack", card, new Vector2(0f, 128f), 44f, tackSprite);

            // Red ruled line under title area
            var stripe              = MakeRect("Stripe", card);
            stripe.anchorMin        = new Vector2(0f, 1f);
            stripe.anchorMax        = new Vector2(1f, 1f);
            stripe.pivot            = new Vector2(0.5f, 1f);
            stripe.sizeDelta        = new Vector2(0f, 5f);
            stripe.anchoredPosition = new Vector2(0f, -52f);
            stripe.gameObject.AddComponent<Image>().color = new Color(0.80f, 0.15f, 0.15f, 0.65f);

            // Title
            var titleRt        = MakeRect("Title", card);
            titleRt.anchorMin  = new Vector2(0f, 0.58f);
            titleRt.anchorMax  = new Vector2(1f, 1f);
            titleRt.offsetMin  = new Vector2(14f, 0f);
            titleRt.offsetMax  = new Vector2(-14f, -50f);
            var titleTmp       = titleRt.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text      = lead.title;
            titleTmp.fontSize  = 28f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color     = new Color(0.10f, 0.05f, 0.02f, 1f);
            titleTmp.alignment = TextAlignmentOptions.TopLeft;
            titleTmp.raycastTarget = false;

            // Subtitle
            var subRt        = MakeRect("Sub", card);
            subRt.anchorMin  = new Vector2(0f, 0f);
            subRt.anchorMax  = new Vector2(1f, 0.58f);
            subRt.offsetMin  = new Vector2(14f, 10f);
            subRt.offsetMax  = new Vector2(-14f, 0f);
            var subTmp       = subRt.gameObject.AddComponent<TextMeshProUGUI>();
            string sub       = lead.subtitle ?? string.Empty;
            subTmp.text      = sub.Length > 90 ? sub.Substring(0, 87) + "…" : sub;
            subTmp.fontSize  = 21f;
            subTmp.color     = new Color(0.25f, 0.18f, 0.10f, 1f);
            subTmp.alignment = TextAlignmentOptions.TopLeft;
            subTmp.raycastTarget = false;

            // "Tap to replay" affordance — bottom-right corner, quiet
            var replayRt        = MakeRect("Replay", card);
            replayRt.anchorMin  = new Vector2(0f, 0f);
            replayRt.anchorMax  = new Vector2(1f, 0f);
            replayRt.pivot      = new Vector2(0.5f, 0f);
            replayRt.sizeDelta  = new Vector2(-28f, 30f);
            replayRt.anchoredPosition = new Vector2(0f, 6f);
            var replayTmp       = replayRt.gameObject.AddComponent<TextMeshProUGUI>();
            replayTmp.text      = "▸ replay";
            replayTmp.fontSize  = 18f;
            replayTmp.fontStyle = FontStyles.Italic;
            replayTmp.color     = new Color(0.45f, 0.32f, 0.18f, 0.85f);
            replayTmp.alignment = TextAlignmentOptions.BottomRight;
            replayTmp.raycastTarget = false;

            return card;
        }

        internal static void AddShadow(RectTransform card)
        {
            var rt        = MakeRect("Shadow", card);
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = new Vector2(10f, -12f);
            rt.SetAsFirstSibling();
            var img           = rt.gameObject.AddComponent<Image>();
            img.color         = new Color(0f, 0f, 0f, 0.28f);
            img.raycastTarget = false;
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
            else                { img.color  = new Color(0.20f, 0.50f, 0.90f, 1f); }
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

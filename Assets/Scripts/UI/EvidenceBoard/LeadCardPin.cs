using System;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    [RequireComponent(typeof(Image))]
    public class LeadCardPin : MonoBehaviour, IPointerClickHandler
    {
        private LeadData _lead;
        private Action<LeadData> _onTap;

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

            return card;
        }

        public void OnPointerClick(PointerEventData eventData) => _onTap?.Invoke(_lead);

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

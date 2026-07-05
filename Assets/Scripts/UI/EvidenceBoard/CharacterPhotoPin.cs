using System;
using System.Collections.Generic;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    [RequireComponent(typeof(Image))]
    public class CharacterPhotoPin : MonoBehaviour, IPointerClickHandler
    {
        private LeadData _lead;
        private List<LeadData> _relatedLeads;
        private Action<LeadData> _onReplay;

        public static RectTransform Create(RectTransform parent, LeadData lead,
            List<LeadData> relatedLeads, Vector2 pos, Action<LeadData> onReplay,
            Sprite tackSprite = null, string displayName = null)
        {
            var card              = MakeRect("Photo_" + lead.leadId, parent);
            card.anchorMin        = new Vector2(0.5f, 0.5f);
            card.anchorMax        = new Vector2(0.5f, 0.5f);
            card.pivot            = new Vector2(0.5f, 0.5f);
            card.sizeDelta        = new Vector2(260f, 310f);
            card.anchoredPosition = pos;
            card.localRotation    = Quaternion.Euler(0f, 0f, Tilt(lead.leadId + "_ph"));
            card.gameObject.AddComponent<Image>().color = new Color(0.97f, 0.97f, 0.95f, 1f);

            var pin              = card.gameObject.AddComponent<CharacterPhotoPin>();
            pin._lead            = lead;
            pin._relatedLeads    = relatedLeads;
            pin._onReplay        = onReplay;

            // Thumbtack
            AddTack("Tack", card, new Vector2(0f, 133f), 44f, tackSprite);

            // Portrait area
            var pRt              = MakeRect("Portrait", card);
            pRt.anchorMin        = new Vector2(0.08f, 0.22f);
            pRt.anchorMax        = new Vector2(0.92f, 0.88f);
            pRt.offsetMin        = pRt.offsetMax = Vector2.zero;
            var pImg             = pRt.gameObject.AddComponent<Image>();
            pImg.sprite          = lead.actorPortrait;
            pImg.preserveAspect  = true;
            pImg.raycastTarget   = false;
            if (lead.actorPortrait == null)
                pImg.color = new Color(0.65f, 0.65f, 0.65f, 1f);

            // Name label
            var nRt              = MakeRect("Name", card);
            nRt.anchorMin        = new Vector2(0f, 0f);
            nRt.anchorMax        = new Vector2(1f, 0.22f);
            nRt.offsetMin        = new Vector2(6f, 4f);
            nRt.offsetMax        = new Vector2(-6f, 0f);
            var nTmp             = nRt.gameObject.AddComponent<TextMeshProUGUI>();
            nTmp.text            = displayName ?? lead.title;
            nTmp.fontSize        = 21f;
            nTmp.fontStyle       = FontStyles.Bold;
            nTmp.color           = new Color(0.10f, 0.05f, 0.02f, 1f);
            nTmp.alignment       = TextAlignmentOptions.Center;
            nTmp.raycastTarget   = false;

            return card;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CharacterProfileModal.Show(_lead, _relatedLeads, _onReplay);
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
            return (float)(rng.NextDouble() * 8.0 - 4.0);
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }
    }
}

using System;
using System.Collections.Generic;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    /// <summary>
    /// Polaroid of a location, cropped from its stage background (partial view
    /// by design — the wide view is a purchasable history reward). Taps arrive
    /// via the board's raw-input poll, same as the character pins.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LocationPhotoPin : MonoBehaviour
    {
        private string _key;
        private Sprite _bg;
        private List<BoardScene> _scenes;
        private Action<BoardScene> _onReplay;

        public void Tap() => LocationModal.Show(_key, _bg, _scenes, _onReplay);

        public static RectTransform Create(RectTransform parent, string key, Sprite bg,
            List<BoardScene> scenes, Vector2 pos, Action<BoardScene> onReplay, Sprite tackSprite)
        {
            var def  = LocationCatalog.Get(key);
            var card              = MakeRect("Loc_" + key, parent);
            card.anchorMin        = new Vector2(0.5f, 0.5f);
            card.anchorMax        = new Vector2(0.5f, 0.5f);
            card.pivot            = new Vector2(0.5f, 0.5f);
            card.sizeDelta        = new Vector2(280f, 330f);
            card.anchoredPosition = pos;
            card.localRotation    = Quaternion.Euler(0f, 0f, Tilt(key + "_loc"));
            card.gameObject.AddComponent<Image>().color = new Color(0.97f, 0.97f, 0.95f, 1f);

            var pin       = card.gameObject.AddComponent<LocationPhotoPin>();
            pin._key      = key;
            pin._bg       = bg;
            pin._scenes   = scenes;
            pin._onReplay = onReplay;

            LeadCardPin.AddShadow(card);
            AddTack(card, tackSprite);

            // Partial crop of the background. RawImage + uvRect keeps it a
            // view into the existing render — no new texture memory.
            var pRt       = MakeRect("Photo", card);
            pRt.anchorMin = new Vector2(0.07f, 0.24f);
            pRt.anchorMax = new Vector2(0.93f, 0.90f);
            pRt.offsetMin = pRt.offsetMax = Vector2.zero;
            if (bg != null)
            {
                var raw           = pRt.gameObject.AddComponent<RawImage>();
                raw.texture       = bg.texture;
                raw.uvRect        = CropRect(bg, 0.62f, 0.58f);
                raw.raycastTarget = false;
            }
            else
            {
                var img           = pRt.gameObject.AddComponent<Image>();
                img.color         = new Color(0.72f, 0.66f, 0.55f, 1f);
                img.raycastTarget = false;
            }

            var nRt       = MakeRect("Name", card);
            nRt.anchorMin = new Vector2(0f, 0f);
            nRt.anchorMax = new Vector2(1f, 0.24f);
            nRt.offsetMin = new Vector2(6f, 4f);
            nRt.offsetMax = new Vector2(-6f, 0f);
            var nTmp           = nRt.gameObject.AddComponent<TextMeshProUGUI>();
            nTmp.text          = def != null ? def.displayName : key;
            nTmp.fontSize      = 21f;
            nTmp.fontStyle     = FontStyles.Bold;
            nTmp.color         = new Color(0.10f, 0.05f, 0.02f, 1f);
            nTmp.alignment     = TextAlignmentOptions.Center;
            nTmp.raycastTarget = false;

            return card;
        }

        /// <summary>Centered square-ish crop in normalized texture UVs, biased
        /// slightly above center where the subject of a scene usually sits.</summary>
        internal static Rect CropRect(Sprite s, float widthFrac, float centerY)
        {
            var tex  = s.texture;
            var tr   = s.textureRect;
            float u0 = tr.x / tex.width, v0 = tr.y / tex.height;
            float uw = tr.width / tex.width, vh = tr.height / tex.height;

            float cw = uw * widthFrac;
            // Square in pixels: convert width-px to height-frac.
            float ch = (tr.width * widthFrac) / tr.height * vh;
            ch = Mathf.Min(ch, vh);

            float cx = u0 + (uw - cw) * 0.5f;
            float cy = v0 + Mathf.Clamp(vh * centerY - ch * 0.5f, 0f, vh - ch);
            return new Rect(cx, cy, cw, ch);
        }

        private static void AddTack(RectTransform parent, Sprite sprite)
        {
            var rt              = MakeRect("Tack", parent);
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(44f, 44f);
            rt.anchoredPosition = new Vector2(0f, 143f);
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

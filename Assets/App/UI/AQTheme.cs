// Assembly: AQ.App
// Purpose: Single source of truth for UI colors, fonts and shared widget
// styling. Every programmatic UI reads from here — change a token, change
// the game. The rounded sprite is generated at runtime so the whole UI gets
// rounded corners before any generated 9-slice art lands.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI
{
    public static class AQTheme
    {
        // ---- Palette (derived from the Ally key art) ----
        public static readonly Color Navy       = Hex("0A1220"); // deep background
        public static readonly Color Panel      = Hex("141C2E"); // popup body
        public static readonly Color PanelLine  = Hex("2A3550"); // borders/dividers
        public static readonly Color Card       = Hex("1D2740"); // inner cards/rows
        public static readonly Color Teal       = Hex("1F8A80"); // primary action
        public static readonly Color TealDim    = Hex("15514C"); // disabled primary
        public static readonly Color Steel      = Hex("3A5A80"); // secondary action
        public static readonly Color SteelDim   = Hex("263A52"); // disabled secondary
        public static readonly Color Amber      = Hex("E8A33D"); // highlights, energy
        public static readonly Color Paper      = Hex("F0EBDF"); // primary text
        public static readonly Color PaperDim   = Hex("A5A092"); // secondary text
        public static readonly Color AlertRed   = Hex("B03A3A"); // danger/close
        public static readonly Color Success    = Hex("3F9D5A"); // checks, confirms
        public static readonly Color Scrim      = new Color(0f, 0f, 0f, 0.78f);

        static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : Color.magenta;
        }

        // ---- Fonts (built by AQ > Setup > Build TMP Font Assets) ----
        static TMP_FontAsset _display, _body;
        public static TMP_FontAsset Display =>
            _display ??= Resources.Load<TMP_FontAsset>("App/UI/Fonts/Staatliches SDF");
        public static TMP_FontAsset Body =>
            _body ??= Resources.Load<TMP_FontAsset>("App/UI/Fonts/NunitoSans SDF");

        /// <summary>Display font for headings/buttons, Body for copy. No-op if fonts missing.</summary>
        public static void StyleText(TMP_Text tmp, bool display = false)
        {
            if (tmp == null) return;
            var f = display ? Display : Body;
            if (f == null) return;
            tmp.font = f;
            // Staatliches has no bold face; synthetic bold smears the SDF.
            if (display) tmp.fontStyle &= ~FontStyles.Bold;
        }

        // ---- Rounded-rect 9-slice sprite (runtime-generated, cached) ----
        const int TexSize = 96;
        const float CornerRadius = 32f;
        const float SliceBorder = 40f;

        static Sprite _rounded;
        public static Sprite Rounded
        {
            get
            {
                if (_rounded != null) return _rounded;

                var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode  = TextureWrapMode.Clamp
                };
                var px   = new Color32[TexSize * TexSize];
                float half = TexSize / 2f;
                for (int y = 0; y < TexSize; y++)
                for (int x = 0; x < TexSize; x++)
                {
                    // signed distance to a rounded rect centred in the texture
                    float cx = Mathf.Abs(x + 0.5f - half) - (half - CornerRadius);
                    float cy = Mathf.Abs(y + 0.5f - half) - (half - CornerRadius);
                    float d  = new Vector2(Mathf.Max(cx, 0f), Mathf.Max(cy, 0f)).magnitude
                               + Mathf.Min(Mathf.Max(cx, cy), 0f) - CornerRadius;
                    byte a = (byte)(Mathf.Clamp01(0.5f - d) * 255f);
                    px[y * TexSize + x] = new Color32(255, 255, 255, a);
                }
                tex.SetPixels32(px);
                tex.Apply(false, true);

                _rounded = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                    new Vector4(SliceBorder, SliceBorder, SliceBorder, SliceBorder));
                _rounded.name = "AQTheme_Rounded";
                _rounded.hideFlags = HideFlags.HideAndDontSave;
                return _rounded;
            }
        }

        /// <summary>Give an Image rounded corners and a theme color.</summary>
        public static Image Round(Image img, Color color)
        {
            img.sprite = Rounded;
            img.type   = Image.Type.Sliced;
            img.color  = color;
            return img;
        }

        /// <summary>
        /// Popup panel treatment: hairline border (image on the rect itself)
        /// with an inset body. Call before adding content so the body renders
        /// underneath everything else.
        /// </summary>
        public static void StylePanel(RectTransform panel)
        {
            Round(panel.gameObject.AddComponent<Image>(), PanelLine);

            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(panel, false);
            var rt = (RectTransform)body.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(3f, 3f);
            rt.offsetMax = new Vector2(-3f, -3f);
            Round(body.GetComponent<Image>(), Panel);
            body.GetComponent<Image>().raycastTarget = false;
        }
    }
}

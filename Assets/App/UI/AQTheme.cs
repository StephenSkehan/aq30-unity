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

        // Board
        public static readonly Color BoardFrame = Hex("101A2C"); // plate behind the grid
        public static readonly Color BoardCellA = Hex("1D2740"); // checker light
        public static readonly Color BoardCellB = Hex("18223A"); // checker dark

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

                // Prefer the persisted asset (scene objects share it); generate as fallback.
                _rounded = Resources.Load<Sprite>("App/UI/aq_rounded");
                if (_rounded != null) return _rounded;

                var tex = BuildRoundedTexture(false);
                _rounded = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                    new Vector4(SliceBorder, SliceBorder, SliceBorder, SliceBorder));
                _rounded.name = "AQTheme_Rounded";
                _rounded.hideFlags = HideFlags.HideAndDontSave;
                return _rounded;
            }
        }

        /// <summary>
        /// White rounded-rect texture. readable=true keeps CPU pixels so the
        /// editor can EncodeToPNG it into the persisted Resources asset
        /// (import with a 40px 9-slice border to match SliceBorder).
        /// </summary>
        public static Texture2D BuildRoundedTexture(bool readable)
        {
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
            tex.Apply(false, !readable);
            return tex;
        }

        /// <summary>Give an Image rounded corners and a theme color.</summary>
        public static Image Round(Image img, Color color)
        {
            img.sprite = Rounded;
            img.type   = Image.Type.Sliced;
            img.color  = color;
            return img;
        }

        /// <summary>Springy scale-in for popup panels. Safe to call on every show.</summary>
        public static void PopIn(RectTransform panel)
        {
            if (panel == null) return;
            var driver = panel.GetComponent<PopInDriver>();
            if (driver == null) driver = panel.gameObject.AddComponent<PopInDriver>();
            driver.Play();
        }

        private sealed class PopInDriver : MonoBehaviour
        {
            Coroutine _co;

            public void Play()
            {
                if (_co != null) StopCoroutine(_co);
                _co = StartCoroutine(Co());
            }

            System.Collections.IEnumerator Co()
            {
                var rt = (RectTransform)transform;
                const float up = 0.11f, settle = 0.07f;
                float t = 0f;
                while (t < up)
                {
                    t += Time.unscaledDeltaTime;
                    rt.localScale = Vector3.one * Mathf.Lerp(0.86f, 1.04f, Mathf.Clamp01(t / up));
                    yield return null;
                }
                t = 0f;
                while (t < settle)
                {
                    t += Time.unscaledDeltaTime;
                    rt.localScale = Vector3.one * Mathf.Lerp(1.04f, 1f, Mathf.Clamp01(t / settle));
                    yield return null;
                }
                rt.localScale = Vector3.one;
                _co = null;
            }
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

        /// <summary>
        /// Distinct-tone title bar across a popup panel's top (Stephen-ruled
        /// 2026-08-12): centred display title, X close button on the right,
        /// and an optional ? button on the left that toggles a help strip
        /// below the bar — instructional copy lives behind the ?, not as
        /// clutter on the main surface. Call AFTER StylePanel; add content
        /// beneath the bar's height.
        /// </summary>
        /// <summary>Raised when a title bar builds a ? help button (first-open hint hook).</summary>
        public static event System.Action<RectTransform> HelpBarBuilt;

        public static RectTransform TitleBar(RectTransform panel, string title,
            UnityEngine.Events.UnityAction onClose, string helpText = null, float height = 96f)
        {
            var bar = new GameObject("TitleBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(panel, false);
            var rt       = (RectTransform)bar.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(3f, -height - 3f);
            rt.offsetMax = new Vector2(-3f, -3f);
            Round(bar.GetComponent<Image>(), SteelDim); // distinct tone vs the Panel body

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(rt, false);
            var trt       = (RectTransform)titleGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(84f, 0f);   // clear the corner buttons
            trt.offsetMax = new Vector2(-84f, 0f);
            var tmp           = titleGo.AddComponent<TextMeshProUGUI>();
            tmp.text          = title;
            tmp.fontSize      = 42f;
            tmp.color         = Paper;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            StyleText(tmp, display: true);

            if (onClose != null)
            {
                var x = BarButton(rt, new Vector2(1f, 0.5f), new Vector2(-16f, 0f));
                AddDrawnX((RectTransform)x.transform, Color.white, 30f, 6f);
                x.onClick.AddListener(onClose);
            }

            if (!string.IsNullOrEmpty(helpText))
            {
                // Hidden help strip just under the bar; the ? toggles it.
                var strip = new GameObject("HelpStrip", typeof(RectTransform), typeof(Image));
                strip.transform.SetParent(panel, false);
                var srt       = (RectTransform)strip.transform;
                srt.anchorMin = new Vector2(0f, 1f);
                srt.anchorMax = new Vector2(1f, 1f);
                srt.pivot     = new Vector2(0.5f, 1f);
                srt.offsetMin = new Vector2(12f, -height - 9f - 118f);
                srt.offsetMax = new Vector2(-12f, -height - 9f);
                Round(strip.GetComponent<Image>(), Card);

                var txtGo = new GameObject("Text", typeof(RectTransform));
                txtGo.transform.SetParent(srt, false);
                var xrt       = (RectTransform)txtGo.transform;
                xrt.anchorMin = Vector2.zero;
                xrt.anchorMax = Vector2.one;
                xrt.offsetMin = new Vector2(20f, 10f);
                xrt.offsetMax = new Vector2(-20f, -10f);
                var htmp           = txtGo.AddComponent<TextMeshProUGUI>();
                htmp.text          = helpText;
                htmp.fontSize      = 26f;
                htmp.color         = PaperDim;
                htmp.alignment     = TextAlignmentOptions.Center;
                htmp.raycastTarget = false;
                StyleText(htmp);
                strip.SetActive(false);

                var help = BarButton(rt, new Vector2(0f, 0.5f), new Vector2(16f, 0f));
                var qGo  = new GameObject("Q", typeof(RectTransform));
                qGo.transform.SetParent(help.transform, false);
                var qrt       = (RectTransform)qGo.transform;
                qrt.anchorMin = Vector2.zero;
                qrt.anchorMax = Vector2.one;
                qrt.offsetMin = qrt.offsetMax = Vector2.zero;
                var qtmp           = qGo.AddComponent<TextMeshProUGUI>();
                qtmp.text          = "?";
                qtmp.fontSize      = 38f;
                qtmp.color         = Color.white;
                qtmp.alignment     = TextAlignmentOptions.Center;
                qtmp.raycastTarget = false;
                StyleText(qtmp, display: true);
                help.onClick.AddListener(() => strip.SetActive(!strip.activeSelf));
                HelpBarBuilt?.Invoke((RectTransform)help.transform);
            }

            return rt;
        }

        private static Button BarButton(RectTransform bar, Vector2 sideAnchor, Vector2 offset)
        {
            var go = new GameObject("BarBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(bar, false);
            var rt              = (RectTransform)go.transform;
            rt.anchorMin        = rt.anchorMax = sideAnchor;
            rt.pivot            = sideAnchor;
            rt.sizeDelta        = new Vector2(64f, 64f);
            rt.anchoredPosition = offset;
            StyleButton(go.GetComponent<Image>(), Steel);
            return go.GetComponent<Button>();
        }

        // ── layered buttons ──────────────────────────────────────────────────

        private static Color RimColor(Color baseColor) => Color.Lerp(baseColor, Color.white, 0.30f);

        /// <summary>
        /// Layered button treatment: soft drop shadow, lit rim, inset body,
        /// top sheen + bottom shade, and press tinting. Call on the button's
        /// root Image BEFORE adding the label so the text renders on top.
        /// Retint later (tab highlights) with RetintButton.
        /// </summary>
        public static Image StyleButton(Image img, Color baseColor)
        {
            Round(img, RimColor(baseColor));

            var shadow = img.gameObject.GetComponent<Shadow>();
            if (shadow == null) shadow = img.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -6f);

            var body = new GameObject("BtnBody", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(img.transform, false);
            var brt = (RectTransform)body.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(2.5f, 2.5f);
            brt.offsetMax = new Vector2(-2.5f, -2.5f);
            var bodyImg = Round(body.GetComponent<Image>(), baseColor);
            bodyImg.raycastTarget = false;

            var sheen = new GameObject("Sheen", typeof(RectTransform), typeof(Image));
            sheen.transform.SetParent(body.transform, false);
            var shr = (RectTransform)sheen.transform;
            shr.anchorMin = new Vector2(0f, 0.52f);
            shr.anchorMax = new Vector2(1f, 1f);
            shr.offsetMin = new Vector2(4f, 0f);
            shr.offsetMax = new Vector2(-4f, -3f);
            var sheenImg = Round(sheen.GetComponent<Image>(), new Color(1f, 1f, 1f, 0.10f));
            sheenImg.raycastTarget = false;

            var shade = new GameObject("Shade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(body.transform, false);
            var sdr = (RectTransform)shade.transform;
            sdr.anchorMin = new Vector2(0f, 0f);
            sdr.anchorMax = new Vector2(1f, 0.34f);
            sdr.offsetMin = new Vector2(4f, 3f);
            sdr.offsetMax = new Vector2(-4f, 0f);
            var shadeImg = Round(shade.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.16f));
            shadeImg.raycastTarget = false;

            var btn = img.GetComponent<Button>();
            if (btn != null)
            {
                btn.transition    = Selectable.Transition.ColorTint;
                btn.targetGraphic = bodyImg;
                var cb = btn.colors;
                cb.normalColor      = Color.white;
                cb.highlightedColor = Color.white;
                cb.pressedColor     = new Color(0.78f, 0.78f, 0.78f, 1f);
                cb.selectedColor    = Color.white;
                cb.fadeDuration     = 0.06f;
                btn.colors = cb;
            }

            return bodyImg;
        }

        /// <summary>Retint a StyleButton-treated button (rim + body) in place.</summary>
        public static void RetintButton(Button btn, Color baseColor)
        {
            if (btn == null) return;
            var root = btn.GetComponent<Image>();
            if (root != null) root.color = RimColor(baseColor);
            var body = btn.transform.Find("BtnBody");
            if (body != null)
            {
                var bi = body.GetComponent<Image>();
                if (bi != null) bi.color = baseColor;
            }
        }

        /// <summary>Drawn ✕ glyph (two rotated rounded bars) — the game font
        /// has no ✕ character, so a TMP "✕" renders as a tofu box.</summary>
        public static void AddDrawnX(RectTransform parent, Color color, float length, float thickness)
        {
            for (int i = 0; i < 2; i++)
            {
                var bar = new GameObject(i == 0 ? "XBarA" : "XBarB", typeof(RectTransform), typeof(Image));
                bar.transform.SetParent(parent, false);
                var rt = (RectTransform)bar.transform;
                rt.anchorMin     = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot         = new Vector2(0.5f, 0.5f);
                rt.sizeDelta     = new Vector2(length, thickness);
                rt.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 45f : -45f);
                var img = Round(bar.GetComponent<Image>(), color);
                img.raycastTarget = false;
            }
        }
    }
}

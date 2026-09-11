using System;
using System.Collections.Generic;
using AQ.App.Leads;
using AQ.SharedKernel.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    /// <summary>
    /// Location sheet (CharacterProfileModal + DossierPopup patterns): hero
    /// crop, epigraph, replayable scenes staged here, and the purchasable
    /// location history. The last detail also unlocks the WIDE VIEW — the
    /// full background render as a full-bleed viewer.
    /// </summary>
    public static class LocationModal
    {
        private static GameObject _root;
        public static bool IsOpen => _root != null;

        public static void Show(string key, Sprite bg, List<BoardScene> scenes, Action<BoardScene> onReplay)
        {
            Close();
            var def = LocationCatalog.Get(key);
            if (def == null) return;

            _root = new GameObject("__LocationModal", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(GraphicRaycaster));
            var canvas          = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var scaler                 = _root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(_root.transform, false);
            Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = AQTheme.Scrim;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_root.transform, false);
            var prt       = (RectTransform)panel.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot     = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(940f, 1680f); // taller rows since the unified 96px buttons
            AQTheme.StylePanel(prt);

            AQTheme.TitleBar(prt, def.displayName.ToUpperInvariant(), Close,
                "Replay any scene set here. The location history reveals what Ally knows about the place; further details cost CaseCash. The last detail unlocks the wide view.");

            float y = -120f; // below the title bar

            // Hero crop band
            if (bg != null)
            {
                var hero = new GameObject("Hero", typeof(RectTransform), typeof(RawImage));
                hero.transform.SetParent(prt, false);
                var hrt       = (RectTransform)hero.transform;
                hrt.anchorMin = new Vector2(0f, 1f);
                hrt.anchorMax = new Vector2(1f, 1f);
                hrt.pivot     = new Vector2(0.5f, 1f);
                hrt.offsetMin = new Vector2(24f, y - 300f);
                hrt.offsetMax = new Vector2(-24f, y);
                var raw           = hero.GetComponent<RawImage>();
                raw.texture       = bg.texture;
                raw.uvRect        = WideBandCrop(bg);
                raw.raycastTarget = false;
                y -= 312f;
            }

            y = AddText(prt, def.epigraph, 30f, new Color(0.72f, 0.68f, 0.58f, 1f), y, italic: true) - 10f;

            // Scenes here
            y = AddText(prt, "SCENES HERE", 26f, AQTheme.Teal, y, display: true) - 4f;
            int shown = 0;
            foreach (var lead in scenes)
            {
                if (shown >= 4) break; // taller rows: cap keeps the sheet inside the panel
                var captured = lead;
                // No ▸ glyph: NunitoSans renders it as tofu (console warning history).
                y = AddRowButton(prt, lead.title, y, () => { Close(); onReplay?.Invoke(captured); });
                shown++;
            }
            if (scenes.Count > shown)
                y = AddText(prt, $"and {scenes.Count - shown} more as the case grows.", 22f,
                    new Color(0.6f, 0.56f, 0.48f, 1f), y);

            // Location history
            y = AddText(prt, "LOCATION HISTORY", 26f, AQTheme.Teal, y - 14f, display: true) - 4f;
            int unlocked = LocationService.UnlockedCount(key);
            for (int i = 0; i < unlocked && i < def.details.Length; i++)
                y = AddPaper(prt, def.details[i].text, y);

            if (unlocked < def.details.Length)
            {
                var next   = def.details[unlocked];
                var wallet = Economy.WalletLocator.Instance;
                bool canAfford = wallet != null && wallet.Get(Currency.Soft) >= next.price;
                string label = $"UNLOCK  ·  {next.price} CC" + (next.wideView ? "  ·  WIDE VIEW" : "");
                y = AddActionButton(prt, label, y,
                    canAfford ? new Color(0.55f, 0.40f, 0.16f, 1f) : new Color(0.35f, 0.33f, 0.30f, 1f),
                    () =>
                    {
                        if (LocationService.TryUnlockNext(key))
                            Show(key, bg, scenes, onReplay); // rebuild with the new entry
                        else
                            UI.Common.ToastService.Show("loc_cc", "Not enough CaseCash.", 2f);
                    });
            }
            else if (bg != null)
            {
                y = AddActionButton(prt, "VIEW THE WIDE SHOT", y, new Color(0.18f, 0.42f, 0.28f, 1f),
                    () => ShowWideView(bg));
            }
        }

        public static void Close()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        // Full-bleed viewer of the complete background render. Tap closes.
        private static void ShowWideView(Sprite bg)
        {
            var viewer = new GameObject("WideView", typeof(RectTransform), typeof(Image), typeof(Button));
            viewer.transform.SetParent(_root.transform, false);
            Stretch((RectTransform)viewer.transform);
            viewer.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.03f, 0.98f);
            viewer.GetComponent<Button>().onClick.AddListener(() => UnityEngine.Object.Destroy(viewer));

            var img = new GameObject("Img", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            img.transform.SetParent(viewer.transform, false);
            Stretch((RectTransform)img.transform);
            var i             = img.GetComponent<Image>();
            i.sprite          = bg;
            i.preserveAspect  = true;
            i.raycastTarget   = false;
            var fit           = img.GetComponent<AspectRatioFitter>();
            fit.aspectMode    = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio   = bg.rect.width / bg.rect.height;

            var hint = new GameObject("Hint", typeof(RectTransform));
            hint.transform.SetParent(viewer.transform, false);
            var hrt       = (RectTransform)hint.transform;
            hrt.anchorMin = new Vector2(0f, 0f);
            hrt.anchorMax = new Vector2(1f, 0f);
            hrt.pivot     = new Vector2(0.5f, 0f);
            hrt.sizeDelta = new Vector2(0f, 70f);
            var tmp           = hint.AddComponent<TextMeshProUGUI>();
            tmp.text          = "tap to close";
            tmp.fontSize      = 24f;
            tmp.color         = new Color(0.7f, 0.66f, 0.58f, 0.8f);
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);
        }

        // Wide letterbox crop across the middle of the render for the hero band.
        private static Rect WideBandCrop(Sprite s)
        {
            var tex  = s.texture;
            var tr   = s.textureRect;
            float u0 = tr.x / tex.width, v0 = tr.y / tex.height;
            float uw = tr.width / tex.width, vh = tr.height / tex.height;
            // Band aspect is 892x300; height in UV from pixel aspect.
            float ch = Mathf.Min(vh, (tr.width * (300f / 892f)) / tr.height * vh);
            float cy = v0 + Mathf.Clamp(vh * 0.62f - ch * 0.5f, 0f, vh - ch);
            return new Rect(u0, cy, uw, ch);
        }

        // ---- small builders ----

        private static float AddText(RectTransform parent, string text, float size, Color color,
            float y, bool display = false, bool italic = false)
        {
            var go = new GameObject("Txt", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt        = (RectTransform)go.transform;
            rt.anchorMin  = new Vector2(0f, 1f);
            rt.anchorMax  = new Vector2(1f, 1f);
            rt.pivot      = new Vector2(0.5f, 1f);
            rt.offsetMin  = new Vector2(40f, y - 44f);
            rt.offsetMax  = new Vector2(-40f, y);
            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = size;
            tmp.color         = color;
            tmp.alignment     = TextAlignmentOptions.MidlineLeft;
            if (italic) tmp.fontStyle = FontStyles.Italic;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: display);
            return y - 48f;
        }

        private static float AddPaper(RectTransform parent, string text, float y)
        {
            var go = new GameObject("Paper", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt       = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(32f, y - 104f);
            rt.offsetMax = new Vector2(-32f, y);
            var img      = go.GetComponent<Image>();
            img.sprite   = AQTheme.Rounded;
            img.type     = Image.Type.Sliced;
            img.color    = new Color(0.93f, 0.90f, 0.82f, 1f);
            img.raycastTarget = false;

            var txt = new GameObject("T", typeof(RectTransform));
            txt.transform.SetParent(rt, false);
            var trt       = (RectTransform)txt.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(22f, 10f);
            trt.offsetMax = new Vector2(-22f, -10f);
            var tmp           = txt.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = 25f;
            tmp.color         = new Color(0.16f, 0.12f, 0.07f, 1f);
            tmp.alignment     = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);
            return y - 112f;
        }

        // One geometry for every button in the sheet (Stephen-ruled 2026-08-14:
        // same height and width, font up 50 percent).
        private const float BtnH = 96f;

        private static float AddRowButton(RectTransform parent, string label, float y, Action onClick)
            => AddButtonRow(parent, label, y, new Color(0.18f, 0.42f, 0.28f, 1f), BtnH, onClick);

        private static float AddActionButton(RectTransform parent, string label, float y, Color color, Action onClick)
            => AddButtonRow(parent, label, y - 8f, color, BtnH, onClick);

        private static float AddButtonRow(RectTransform parent, string label, float y, Color color,
            float h, Action onClick)
        {
            var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt       = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(40f, y - h);
            rt.offsetMax = new Vector2(-40f, y);
            AQTheme.StyleButton(go.GetComponent<Image>(), color);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var txt = new GameObject("T", typeof(RectTransform));
            txt.transform.SetParent(rt, false);
            Stretch((RectTransform)txt.transform);
            var tmp           = txt.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 42f;
            tmp.color         = AQTheme.Paper;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: true);
            return y - h - 10f;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}

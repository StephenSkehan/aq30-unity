using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Dossiers
{
    /// <summary>
    /// A character's case file: free intro, every unlocked entry, and ONLY the
    /// next locked fact (Stephen-ruled 2026-08-11) with its price and reward
    /// visible. Opens from the evidence-board profile modal (CASE FILE button)
    /// and from Ally's HUD bust. Dynamic on-demand canvas — normal Buttons are
    /// safe here (the raw-input poll rule is for boot-created canvases).
    /// </summary>
    public static class DossierPopup
    {
        private static GameObject _root;
        private static Sprite _portrait;

        /// <summary>Board tap polls suspend while a dossier is up.</summary>
        public static bool IsOpen => _root != null;

        public static void Show(string characterKey, Sprite portrait = null)
        {
            if (_root != null || !DossierService.Has(characterKey)) return;
            _portrait = portrait != null ? portrait : _portrait;

            var def      = DossierCatalog.All[characterKey];
            int unlocked = DossierService.UnlockedCount(characterKey);
            var next     = DossierService.NextFact(characterKey);
            bool sealed_ = DossierService.NextIsSealed(characterKey);
            bool done    = DossierService.IsComplete(characterKey);

            const float panelW = 900f;
            const float textW  = panelW - 100f;

            // ---- measure pass ----
            float introH = EstimateTextH(def.intro, 26f, textW) + 8f;
            float panelH = 96f + 16f                             // title bar
                         + (_portrait != null ? 160f + 12f : 0f) // portrait
                         + 2f + 16f                              // divider
                         + introH + 16f;                         // intro
            var factHs = new float[unlocked];
            for (int i = 0; i < unlocked; i++)
            {
                factHs[i] = 30f + EstimateTextH(def.facts[i].text, 26f, textW - 40f) + 26f;
                panelH   += factHs[i] + 12f;
            }
            float nextH = done ? 110f : (sealed_ ? 76f : 196f);
            panelH += nextH + 16f + 80f + 24f;                   // next/complete + close

            // ---- canvas ----
            _root = new GameObject("__Dossier", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);
            var canvas          = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            var scaler                 = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim       = MakeRect("Dim", _root.transform);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            var panel              = MakeRect("Panel", _root.transform);
            panel.anchorMin        = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(panelW, panelH);
            panel.anchoredPosition = Vector2.zero;
            // Rounded corners to match the title bar (Stephen-ruled 2026-08-21).
            AQTheme.Round(panel.gameObject.AddComponent<Image>(), new Color(0.12f, 0.10f, 0.08f, 1f));

            float cursor = panelH / 2f - 96f - 16f;

            // Portrait
            if (_portrait != null)
            {
                var pRt             = PlaceRect("Portrait", panel, new Vector2(160f, 160f), new Vector2(0f, cursor - 80f));
                var pImg            = pRt.gameObject.AddComponent<Image>();
                pImg.sprite         = _portrait;
                pImg.preserveAspect = true;
                pImg.raycastTarget  = false;
                cursor -= 160f + 12f;
            }

            var div = PlaceRect("Divider", panel, new Vector2(panelW - 80f, 2f), new Vector2(0f, cursor - 1f));
            div.gameObject.AddComponent<Image>().color = new Color(0.35f, 0.30f, 0.25f, 1f);
            cursor -= 2f + 16f;

            // Intro (free)
            PlaceText("Intro", panel, textW, introH, ref cursor, def.intro, 26f,
                      new Color(0.72f, 0.68f, 0.64f, 1f), FontStyles.Italic, TextAlignmentOptions.Center);
            cursor -= 16f;

            // Unlocked entries — paper strips from the file
            for (int i = 0; i < unlocked; i++)
            {
                float h    = factHs[i];
                var card   = PlaceRect("Entry" + (i + 1), panel, new Vector2(panelW - 60f, h), new Vector2(0f, cursor - h / 2f));
                card.gameObject.AddComponent<Image>().color = new Color(0.93f, 0.90f, 0.84f, 1f);
                var cap    = PlaceRect("Cap", card, new Vector2(panelW - 100f, 26f), new Vector2(0f, h / 2f - 20f));
                AddTmp(cap, "ENTRY " + (i + 1), 18f, new Color(0.55f, 0.42f, 0.26f, 1f),
                       FontStyles.Bold, TextAlignmentOptions.Left);
                var body   = PlaceRect("Body", card, new Vector2(textW - 40f, h - 56f), new Vector2(0f, -14f));
                AddTmp(body, def.facts[i].text, 26f, new Color(0.14f, 0.12f, 0.10f, 1f),
                       FontStyles.Normal, TextAlignmentOptions.TopLeft);
                cursor -= h + 12f;
            }

            // Next locked fact / sealed / complete
            var box = PlaceRect("Next", panel, new Vector2(panelW - 60f, nextH), new Vector2(0f, cursor - nextH / 2f));
            box.gameObject.AddComponent<Image>().color = new Color(0.17f, 0.145f, 0.10f, 1f);
            if (done)
            {
                var capRt = PlaceRect("Cap", box, new Vector2(textW, 34f), new Vector2(0f, nextH / 2f - 30f));
                AddTmp(capRt, "FILE COMPLETE", 26f, new Color(0.85f, 0.68f, 0.32f, 1f),
                       FontStyles.Bold, TextAlignmentOptions.Center);
                bool taped = !string.IsNullOrEmpty(def.completionCassette);
                var casRt  = PlaceRect("Cas", box, new Vector2(textW, 34f), new Vector2(0f, nextH / 2f - 74f));
                AddTmp(casRt, taped ? "Tip-line cassette: filed in the Case Kit."
                                    : "Tip-line cassette: tape pending.",
                       24f, new Color(0.66f, 0.62f, 0.56f, 1f), FontStyles.Italic, TextAlignmentOptions.Center);
            }
            else if (sealed_)
            {
                var capRt = PlaceRect("Cap", box, new Vector2(textW, 30f), new Vector2(0f, 0f));
                AddTmp(capRt, "ENTRY " + (unlocked + 1) + "  ·  SEALED FOR NOW", 20f,
                       new Color(0.60f, 0.44f, 0.30f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
            }
            else
            {
                var capRt = PlaceRect("Cap", box, new Vector2(textW, 30f), new Vector2(0f, nextH / 2f - 26f));
                AddTmp(capRt, "ENTRY " + (unlocked + 1) + "  ·  LOCKED", 20f,
                       new Color(0.60f, 0.44f, 0.30f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
                var rwRt = PlaceRect("Reward", box, new Vector2(textW, 32f), new Vector2(0f, nextH / 2f - 64f));
                AddTmp(rwRt, "Comes with: " + next.reward.label, 24f,
                       new Color(0.72f, 0.68f, 0.64f, 1f), FontStyles.Normal, TextAlignmentOptions.Center);
                var buy = PlaceButton("UNLOCK  ·  " + next.price + " CC", box,
                                      new Color(0.18f, 0.42f, 0.28f, 1f),
                                      new Vector2(panelW - 160f, 78f), new Vector2(0f, -nextH / 2f + 52f));
                string keyCap = characterKey;
                buy.onClick.AddListener(() =>
                {
                    if (DossierService.TryUnlockNext(keyCap))
                    {
                        Close();
                        Show(keyCap); // rebuild with the fresh entry revealed
                    }
                    else
                        Common.ToastService.Show("dossier_cc", "Not enough CaseCash.", 2.5f);
                });
            }
            cursor -= nextH + 16f;

            // Close lives in the title bar's X now; Other Case Files stands alone.
            var other = PlaceButton("Other Case Files", panel, new Color(0.55f, 0.40f, 0.16f, 1f),
                                    new Vector2(380f, 78f), new Vector2(0f, cursor - 39f));
            other.onClick.AddListener(() => { Close(); DossierIndexPopup.Show(); });

            // Title-bar treatment (Stephen-ruled 2026-08-12).
            AQTheme.TitleBar(panel, def.displayName, Close,
                "Ally's case file on this character. Unlock the next entry with CaseCash; each comes with a reward, and some pages wait for the case to close. Completing a file earns a keepsake.");
        }

        public static void Close()
        {
            if (_root == null) return;
            Object.Destroy(_root);
            _root = null;
        }

        // ---- helpers ----

        /// <summary>Cheap wrapped-height estimate for the measure pass — 1080-wide
        /// reference layout, TMP default font. Errs generous so text never clips.</summary>
        private static float EstimateTextH(string text, float fontSize, float width)
        {
            int charsPerLine = Mathf.Max(16, Mathf.FloorToInt(width / (fontSize * 0.52f)));
            int lines        = Mathf.Max(1, Mathf.CeilToInt((text ?? string.Empty).Length / (float)charsPerLine));
            return lines * (fontSize * 1.35f);
        }

        private static void PlaceText(string name, RectTransform parent, float w, float h, ref float cursor,
                                      string text, float size, Color color, FontStyles style, TextAlignmentOptions align)
        {
            var rt = PlaceRect(name, parent, new Vector2(w, h), new Vector2(0f, cursor - h / 2f));
            AddTmp(rt, text, size, color, style, align);
            cursor -= h;
        }

        private static TextMeshProUGUI AddTmp(RectTransform rt, string text, float size, Color color,
                                              FontStyles style, TextAlignmentOptions align)
        {
            var tmp           = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = size;
            tmp.color         = color;
            tmp.fontStyle     = style;
            tmp.alignment     = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static RectTransform PlaceRect(string name, RectTransform parent, Vector2 size, Vector2 centre)
        {
            var go              = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = centre;
            return rt;
        }

        private static Button PlaceButton(string label, RectTransform parent, Color color, Vector2 size, Vector2 centre)
        {
            var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = centre;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Lbl", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp           = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 28f;
            tmp.color         = Color.white;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return go.GetComponent<Button>();
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }
    }
}

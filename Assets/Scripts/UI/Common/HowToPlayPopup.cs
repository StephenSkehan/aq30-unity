using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI.Board;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// CASE FILE 101 — the replayable how-to-play reference (SAS FTUE I9,
    /// Stephen-ruled 2026-08-21). Four rows walk the whole loop with real game
    /// sprites, fronted by Gerald (the tutorial voice): tap the kit, merge a
    /// pair, the green tick, proceed the lead. Apple guidance: everything the
    /// tutorial taught (or a player dismissed) must stay retrievable forever.
    /// Opened from Settings > Help; safe to show anywhere (own top canvas).
    /// </summary>
    public static class HowToPlayPopup
    {
        private static GameObject _root;

        // Row copy: Gerald copy sheet, all lines Stephen-ruled 2026-08-21
        // (no endearments in player-facing tutorial copy).
        private static readonly string[] RowLines =
        {
            "Tap the kit. Every item helps.",
            "A matching pair. Drag them together.",
            "Green tick? Somebody's waiting on that.",
            "Lead's gone green. Tap it and proceed.",
        };

        public static void Show()
        {
            if (_root != null) return;

            _root = new GameObject("__HowToPlayPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // over settings and the board
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var dim = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(880f, 1120f);
            panel.anchoredPosition = Vector2.zero;

            AQTheme.TitleBar(panel, "CASE FILE 101", Close,
                "The whole game in four moves. Come back any time.");

            // Gerald header — the wise man walks you through (Stephen concept).
            float y = 380f;
            var mentor = AQ.App.UI.Dossiers.DossierPortraits.Find("gerald");
            if (mentor != null)
            {
                var pgo = new GameObject("Mentor", typeof(RectTransform), typeof(Image));
                pgo.transform.SetParent(panel, false);
                var mrt = (RectTransform)pgo.transform;
                mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0.5f);
                mrt.sizeDelta = new Vector2(150f, 150f);
                mrt.anchoredPosition = new Vector2(-300f, y);
                var pimg = pgo.GetComponent<Image>();
                pimg.sprite = mentor;
                pimg.preserveAspect = true;
                pimg.raycastTarget = false;

                // Gerald copy sheet, Stephen-ruled 2026-08-21.
                var cap = AddLabel(panel, "Gerald's notes. Read them twice.", 30f, AQTheme.PaperDim,
                                   new Vector2(90f, y), new Vector2(560f, 110f));
                cap.alignment = TextAlignmentOptions.MidlineLeft;
            }

            var board = UnityEngine.Object.FindFirstObjectByType<MergeBoardController>();
            float rowY = 200f;
            for (int i = 0; i < RowLines.Length; i++)
            {
                BuildRow(panel, i, RowLines[i], board, rowY);
                rowY -= 185f;
            }
        }

        private static void BuildRow(RectTransform panel, int index, string line,
                                     MergeBoardController board, float y)
        {
            var row = MakeRect($"Row{index + 1}", panel);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(800f, 165f);
            row.anchoredPosition = new Vector2(0f, y);
            var bg = row.gameObject.AddComponent<Image>();
            bg.sprite = AQTheme.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0f, 0f, 0f, 0.30f);
            bg.raycastTarget = false;

            // Icon zone (left) — real game art wherever it exists.
            switch (index)
            {
                case 0: // the generator
                    AddIcon(row, GeneratorSprite(board), new Vector2(-310f, 0f), 120f);
                    break;
                case 1: // a matching pair
                    var item = ItemSprite(board);
                    AddIcon(row, item, new Vector2(-350f, 0f), 96f);
                    AddIcon(row, item, new Vector2(-255f, 0f), 96f);
                    break;
                case 2: // the green tick
                    AddIcon(row, Resources.Load<Sprite>("App/UI/Icons/ui_check_green"),
                            new Vector2(-310f, 0f), 100f);
                    break;
                case 3: // the ready (green) lead card, abstracted
                    var chip = AddIcon(row, AQTheme.Rounded, new Vector2(-310f, 0f), 110f);
                    if (chip != null)
                    {
                        chip.type = Image.Type.Sliced;
                        chip.color = AQTheme.Success;
                        chip.preserveAspect = false;
                        var rt = chip.rectTransform;
                        rt.sizeDelta = new Vector2(96f, 130f);
                    }
                    break;
            }

            var lbl = AddLabel(row, line, 32f, AQTheme.Paper,
                               new Vector2(90f, 0f), new Vector2(560f, 140f));
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
        }

        private static Sprite GeneratorSprite(MergeBoardController board)
        {
            if (board == null) return null;
            var so = board.defaultGeneratorType;
            var s = so != null ? so.SpriteForTier(0) : null;
            return s != null ? s : board.generatorSprite;
        }

        private static Sprite ItemSprite(MergeBoardController board)
        {
            if (board == null) return null;
            var so = board.defaultGeneratorType;
            var cfg = so != null ? so.ConfigForTier(0) : null;
            if (cfg?.dropTable != null)
                foreach (var e in cfg.dropTable)
                    if (e.type == AQ.App.Generators.DropType.Item && e.itemTier == 0 &&
                        AQ.App.Generators.DropRoller.IsEligible(e, subGenLocked: true))
                        return board.SpriteForItem(e.itemFamily, 0);
            return board.SpriteForItemTierPublic(0);
        }

        private static void Close()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        // ---- helpers (EnergyOutPopup idiom) ----

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image AddIcon(RectTransform parent, Sprite sprite, Vector2 pos, float size)
        {
            if (sprite == null) return null;
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI AddLabel(RectTransform parent, string text, float size,
                                                Color color, Vector2 pos, Vector2 dims)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = dims;
            rt.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);
            return tmp;
        }
    }

    /// <summary>Settings > Help tab: the CASE FILE 101 entry point (ships in
    /// production, unlike the Debug tab).</summary>
    public static class HelpSettingsTab
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
            => AQ.App.UI.Settings.GameControlPanelMB.RegisterExternalTab("Help", Build, order: 5);

        private static void Build(RectTransform ct)
        {
            var btnGo = new GameObject("CaseFile101Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(ct, false);
            var rt = (RectTransform)btnGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.62f);
            rt.sizeDelta = new Vector2(520f, 110f);
            var img = btnGo.GetComponent<Image>();
            AQTheme.StyleButton(img, AQTheme.Teal);
            btnGo.GetComponent<Button>().onClick.AddListener(HowToPlayPopup.Show);

            var lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            var lrt = (RectTransform)lblGo.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.text = "CASE FILE 101";
            lbl.fontSize = 36f;
            lbl.alignment = TextAlignmentOptions.Center;
            AQTheme.StyleText(lbl, display: true);

            var cap = new GameObject("Caption", typeof(RectTransform));
            cap.transform.SetParent(ct, false);
            var crt = (RectTransform)cap.transform;
            crt.anchorMin = new Vector2(0.1f, 0.30f);
            crt.anchorMax = new Vector2(0.9f, 0.50f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var ctext = cap.AddComponent<TextMeshProUGUI>();
            ctext.text = "How the board works, in four moves.";
            ctext.fontSize = 26f;
            ctext.color = AQTheme.PaperDim;
            ctext.alignment = TextAlignmentOptions.Center;
            AQTheme.StyleText(ctext);
        }
    }
}

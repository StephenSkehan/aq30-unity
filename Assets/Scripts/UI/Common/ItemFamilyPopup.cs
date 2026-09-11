using System.Collections.Generic;
using System.Globalization;
using AQ.App.Items;
using AQ.App.UI.Board;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Full item-family ladder, opened from TileInfoPopup's FAMILY button.
    /// Every tier of the family in a grid; the tier the player long-pressed is
    /// modestly highlighted (amber rim). Undiscovered tiers render as dark
    /// silhouettes with a ??? label (gating via ItemDiscoveryService).
    /// Sits ABOVE TileInfoPopup (sort 10000 vs 9999); closing returns to it.
    /// </summary>
    public static class ItemFamilyPopup
    {
        private static GameObject _root;

        private const int   Columns  = 4;
        private const float CellSize = 148f;
        private const float CellGap  = 12f;

        public static void Show(string family, int currentTier)
        {
            if (_root != null) return;

            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            var defs = new List<ItemDefinitionSO>();
            foreach (var d in board.ItemDefinitions)
                if (d != null && d.family == family) defs.Add(d);
            defs.Sort((a, b) => a.tier.CompareTo(b.tier));
            if (defs.Count == 0) return;

            _root = new GameObject("__ItemFamilyPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // above TileInfoPopup

            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim    = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color  = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            int rows      = (defs.Count + Columns - 1) / Columns;
            float gridW   = Columns * CellSize + (Columns - 1) * CellGap;
            float gridH   = rows * (CellSize + 60f) + (rows - 1) * CellGap; // +60 two-line name strip per cell (2026-08-12)
            float panelH  = 96f + 18f + 40f + 34f + gridH + 28f; // title bar + count + gap (+12, 2026-09-03) + grid

            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(gridW + 80f, panelH);
            panel.anchoredPosition = Vector2.zero;

            float y = panelH / 2f - 96f - 18f;

            // Appearing as a requirement on a live lead card counts as discovered
            // (Stephen-ruled 2026-08-06) — mark BEFORE counting so the header and
            // the cells agree on this very open.
            var checker = AQ.App.Leads.LeadRequirementChecker.Instance;
            if (checker != null)
                foreach (var d in defs)
                    if (checker.IsItemNeeded(d.itemId))
                        ItemDiscoveryService.Mark(family, d.tier);

            int discovered = 0;
            foreach (var d in defs)
                if (ItemDiscoveryService.IsDiscovered(family, d.tier)) discovered++;
            // Raised to sit between the title bar and the first row of items
            // (Stephen, 2026-09-03: it was riding on the tile tops).
            AddLabel($"{discovered} of {defs.Count} discovered", panel, 26f, AQTheme.PaperDim,
                     new Vector2(0f, y - 6f), new Vector2(gridW, 40f));
            y -= 74f;

            for (int i = 0; i < defs.Count; i++)
            {
                int row = i / Columns, col = i % Columns;
                int inRow = Mathf.Min(Columns, defs.Count - row * Columns);
                float x = (col - (inRow - 1) / 2f) * (CellSize + CellGap);
                float cy = y - CellSize / 2f - row * (CellSize + 60f + CellGap);
                BuildCell(panel, defs[i], family, currentTier, new Vector2(x, cy));
            }

            // Title-bar treatment (Stephen-ruled 2026-08-12).
            AQTheme.TitleBar(panel, FormatFamily(family), Close,
                "Every tier in this family. Silhouettes are undiscovered: merge your way up to reveal them, or serve a Search Warrant.");
        }

        private static void BuildCell(RectTransform parent, ItemDefinitionSO def,
                                      string family, int currentTier, Vector2 pos)
        {
            bool isCurrent    = def.tier == currentTier;
            bool isDiscovered = ItemDiscoveryService.IsDiscovered(family, def.tier);

            var cell = MakeRect($"Tier{def.tier}", parent);
            cell.anchorMin        = new Vector2(0.5f, 0.5f);
            cell.anchorMax        = new Vector2(0.5f, 0.5f);
            cell.pivot            = new Vector2(0.5f, 0.5f);
            cell.sizeDelta        = new Vector2(CellSize, CellSize + 60f);
            cell.anchoredPosition = pos;

            // Modest highlight for the pressed tier: amber rim, same Card body.
            var rim = cell.gameObject.AddComponent<Image>();
            AQTheme.Round(rim, isCurrent ? AQTheme.Amber : AQTheme.PanelLine);
            rim.raycastTarget = false;

            var bodyRt = MakeRect("Body", cell);
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(2.5f, 2.5f);
            bodyRt.offsetMax = new Vector2(-2.5f, -2.5f);
            var body = bodyRt.gameObject.AddComponent<Image>();
            AQTheme.Round(body, AQTheme.Card);
            body.raycastTarget = false;

            var iconRt = MakeRect("Icon", cell);
            iconRt.anchorMin        = new Vector2(0.5f, 1f);
            iconRt.anchorMax        = new Vector2(0.5f, 1f);
            iconRt.pivot            = new Vector2(0.5f, 1f);
            iconRt.sizeDelta        = new Vector2(CellSize - 40f, CellSize - 52f);
            iconRt.anchoredPosition = new Vector2(0f, -10f);
            var icon = iconRt.gameObject.AddComponent<Image>();
            icon.sprite         = def.icon;
            icon.preserveAspect = true;
            icon.raycastTarget  = false;
            if (icon.sprite == null)
                icon.color = new Color(1f, 1f, 1f, 0.12f);
            else if (!isDiscovered)
                icon.color = new Color(0.05f, 0.08f, 0.15f, 0.95f); // silhouette

            // Tier tag — bottom centre, above the name strip (Stephen-ruled
            // 2026-08-06; the top-left placement crowded the cell edge).
            var tag = MakeRect("TierTag", cell);
            tag.anchorMin        = new Vector2(0.5f, 0f);
            tag.anchorMax        = new Vector2(0.5f, 0f);
            tag.pivot            = new Vector2(0.5f, 0f);
            tag.sizeDelta        = new Vector2(60f, 24f);
            tag.anchoredPosition = new Vector2(0f, 60f);
            var tagTmp = tag.gameObject.AddComponent<TextMeshProUGUI>();
            tagTmp.text      = $"T{def.tier + 1}";
            tagTmp.fontSize  = 19f;
            tagTmp.color     = isCurrent ? AQTheme.Amber : AQTheme.PaperDim;
            tagTmp.alignment = TextAlignmentOptions.Center;
            tagTmp.raycastTarget = false;
            AQTheme.StyleText(tagTmp, display: true);

            // Name strip under the icon; hidden tiers keep their secret.
            var nameRt = MakeRect("Name", cell);
            nameRt.anchorMin = new Vector2(0f, 0f);
            nameRt.anchorMax = new Vector2(1f, 0f);
            nameRt.pivot     = new Vector2(0.5f, 0f);
            nameRt.sizeDelta = new Vector2(-10f, 58f);
            nameRt.anchoredPosition = new Vector2(0f, 4f);
            var nameTmp = nameRt.gameObject.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = isDiscovered ? def.displayName : "???";
            nameTmp.fontSize  = 16f; // two lines fit the 58px strip (2026-08-12: no more mid-word ellipsis)
            nameTmp.color     = isDiscovered ? AQTheme.Paper : AQTheme.PaperDim;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.textWrappingMode = TextWrappingModes.Normal;
            nameTmp.overflowMode = TextOverflowModes.Ellipsis;
            nameTmp.raycastTarget = false;
            AQTheme.StyleText(nameTmp);
        }

        private static void Close()
        {
            if (_root == null) return;
            Object.Destroy(_root);
            _root = null;
        }

        private static string FormatFamily(string family)
        {
            if (string.IsNullOrEmpty(family)) return "Unknown";
            var words = family.Split('_');
            var ti = CultureInfo.InvariantCulture.TextInfo;
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0) words[i] = ti.ToTitleCase(words[i]);
            return string.Join(" ", words);
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLabel(string text, RectTransform parent, float fontSize, Color color,
                                     Vector2 anchoredPosition, Vector2 sizeDelta, bool bold = false)
        {
            var rt              = MakeRect("Lbl", parent);
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta        = sizeDelta;
            var tmp             = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text            = text;
            tmp.fontSize        = fontSize;
            tmp.color           = color;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.raycastTarget   = false;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            AQTheme.StyleText(tmp, display: bold);
        }

        private static Button MakeButton(string label, RectTransform parent, Color color, Vector2 anchoredPosition)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(280f, 90f);
            rt.anchoredPosition = anchoredPosition;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text           = label;
            tmp.fontSize       = 40f;
            tmp.color          = AQTheme.Paper;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;
            AQTheme.StyleText(tmp, display: true);

            return go.GetComponent<Button>();
        }
    }
}

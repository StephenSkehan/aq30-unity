using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.Generators;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Long-press popup for generator tiles: shows which item families this
    /// generator can currently drop (flag-eligible entries only) with odds.
    /// </summary>
    public static class GeneratorInfoPopup
    {
        private static GameObject _root;

        public static void Show(GeneratorTypeSO type, int tier, Sprite icon)
        {
            if (_root != null || type == null) return;

            var cfg = type.ConfigForTier(tier);
            if (cfg?.dropTable == null || cfg.dropTable.Length == 0) return;

            // Aggregate eligible item weights by family; note sub-gen presence.
            var byFamily = new Dictionary<string, float>();
            float itemTotal = 0f;
            bool hasSubGen = false;
            foreach (var e in cfg.dropTable)
            {
                if (!DropRoller.IsEligible(e, subGenLocked: false)) continue;
                if (e.type == DropType.SubGenerator) { hasSubGen = true; continue; }
                if (e.weight <= 0f) continue;
                byFamily.TryGetValue(e.itemFamily, out var w);
                byFamily[e.itemFamily] = w + e.weight;
                itemTotal += e.weight;
            }

            _root = new GameObject("__GeneratorInfoPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim    = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color  = new Color(0f, 0f, 0f, 0.75f);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            int lineCount = byFamily.Count + (hasSubGen ? 1 : 0);
            float panelH  = 420f + lineCount * 58f;

            var panel = MakeRect("Panel", _root.transform);
            panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 1f);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(600f, panelH);
            panel.anchoredPosition = Vector2.zero;

            float y = panelH / 2f - 60f;
            AddLabel(string.IsNullOrEmpty(type.displayName) ? FormatFamily(type.generatorTypeId) : type.displayName,
                     panel, 46f, Color.white, new Vector2(0f, y), new Vector2(540f, 70f), bold: true);
            y -= 60f;
            AddLabel($"Generator  ·  Tier {tier + 1}", panel, 30f,
                     new Color(0.65f, 0.65f, 0.65f), new Vector2(0f, y), new Vector2(540f, 40f));
            y -= 60f;

            var iconRt              = MakeRect("Icon", panel);
            iconRt.anchorMin        = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax        = new Vector2(0.5f, 0.5f);
            iconRt.pivot            = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta        = new Vector2(160f, 160f);
            iconRt.anchoredPosition = new Vector2(0f, y - 60f);
            var iconImg             = iconRt.gameObject.AddComponent<Image>();
            iconImg.sprite          = icon;
            iconImg.preserveAspect  = true;
            iconImg.raycastTarget   = false;
            if (icon == null) iconImg.color = new Color(1f, 1f, 1f, 0.15f);
            y -= 190f;

            AddLabel("CAN DROP", panel, 28f, new Color(0.80f, 0.75f, 0.55f),
                     new Vector2(0f, y), new Vector2(540f, 40f), bold: true);
            y -= 55f;

            foreach (var kv in byFamily)
            {
                int pct = itemTotal > 0f ? Mathf.RoundToInt(kv.Value / itemTotal * 100f) : 0;
                AddLabel($"{FormatFamily(kv.Key)}  —  {pct}%", panel, 32f,
                         new Color(0.85f, 0.85f, 0.90f), new Vector2(0f, y), new Vector2(540f, 46f));
                y -= 58f;
            }
            if (hasSubGen)
            {
                AddLabel("Bonus generator  —  rare", panel, 32f,
                         new Color(0.75f, 0.85f, 0.75f), new Vector2(0f, y), new Vector2(540f, 46f));
                y -= 58f;
            }

            var ok = MakeButton("OK", panel, new Color(0.18f, 0.52f, 0.35f), new Vector2(0f, -panelH / 2f + 75f));
            ok.onClick.AddListener(Close);
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
            go.GetComponent<Image>().color = color;

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text           = label;
            tmp.fontSize       = 40f;
            tmp.color          = Color.white;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;

            return go.GetComponent<Button>();
        }
    }
}

using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Common
{
    public static class TileInfoPopup
    {
        private static GameObject _root;

        public static void Show(string displayName, Sprite icon, string family, int tier,
                                System.Action onStore = null)
        {
            if (_root != null) return;

            _root = new GameObject("__TileInfoPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            // Dim overlay
            var dim    = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color  = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            // Panel
            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(600f, 680f);
            panel.anchoredPosition = Vector2.zero;

            // Title
            AddLabel(displayName, panel, 52f, AQTheme.Paper, new Vector2(0f, 270f), new Vector2(540f, 80f), bold: true);

            // Icon
            var iconRt             = MakeRect("Icon", panel);
            iconRt.anchorMin       = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax       = new Vector2(0.5f, 0.5f);
            iconRt.pivot           = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta       = new Vector2(260f, 260f);
            iconRt.anchoredPosition = new Vector2(0f, 70f);
            var iconImg            = iconRt.gameObject.AddComponent<Image>();
            iconImg.sprite         = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget  = false;
            if (icon == null) iconImg.color = new Color(1f, 1f, 1f, 0.15f);

            // Family
            AddLabel($"Family:  {FormatFamily(family)}", panel, 34f,
                     AQTheme.PaperDim, new Vector2(0f, -115f), new Vector2(520f, 50f));

            // Tier
            AddLabel($"Tier:  {tier + 1}", panel, 34f,
                     AQTheme.PaperDim, new Vector2(0f, -170f), new Vector2(520f, 50f));

            // Buttons: OK alone when no store action; OK + STORE side by side otherwise.
            if (onStore == null)
            {
                var ok = MakeButton("OK", panel, AQTheme.Teal, new Vector2(0f, -270f));
                ok.onClick.AddListener(Close);
            }
            else
            {
                var ok = MakeButton("OK", panel, AQTheme.Teal, new Vector2(-150f, -270f));
                ok.onClick.AddListener(Close);

                var store = MakeButton("STORE", panel, AQTheme.Steel, new Vector2(150f, -270f));
                store.onClick.AddListener(() => { onStore(); Close(); });
            }
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
            var rt              = MakeRect(text, parent);
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
            AQTheme.Round(go.GetComponent<Image>(), color);

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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Minimal programmatic yes/no modal (AQTheme styled). First use: confirming
    /// that a lead may consume items from the Evidence Locker.
    /// </summary>
    public static class ConfirmPopup
    {
        private static GameObject _root;

        public static void Show(string title, string message, string confirmLabel,
                                System.Action onConfirm, System.Action onCancel = null)
        {
            if (_root != null) return;

            _root = new GameObject("__ConfirmPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            dimImg.color  = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(680f, 460f);
            panel.anchoredPosition = Vector2.zero;

            AddLabel(panel, title, 48f, AQTheme.Paper, new Vector2(0f, 150f), new Vector2(620f, 70f), display: true);
            AddLabel(panel, message, 32f, AQTheme.PaperDim, new Vector2(0f, 30f), new Vector2(600f, 150f));

            var confirm = MakeButton(panel, confirmLabel, AQTheme.Teal, new Vector2(-160f, -150f));
            confirm.onClick.AddListener(() => { Close(); onConfirm?.Invoke(); });

            var cancel = MakeButton(panel, "CANCEL", AQTheme.Steel, new Vector2(160f, -150f));
            cancel.onClick.AddListener(() => { Close(); onCancel?.Invoke(); });
        }

        private static void Close()
        {
            if (_root == null) return;
            Object.Destroy(_root);
            _root = null;
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLabel(RectTransform parent, string text, float size, Color color,
                                     Vector2 pos, Vector2 dims, bool display = false)
        {
            var rt              = MakeRect("Lbl", parent);
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = dims;
            var tmp             = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text            = text;
            tmp.fontSize        = size;
            tmp.color           = color;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.raycastTarget   = false;
            AQTheme.StyleText(tmp, display: display);
        }

        private static Button MakeButton(RectTransform parent, string label, Color color, Vector2 pos)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(280f, 90f);
            rt.anchoredPosition = pos;
            AQTheme.Round(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp           = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 38f;
            tmp.color         = AQTheme.Paper;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: true);

            return go.GetComponent<Button>();
        }
    }
}

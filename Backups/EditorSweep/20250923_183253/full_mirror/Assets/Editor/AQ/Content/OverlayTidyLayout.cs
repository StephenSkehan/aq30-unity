#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.EditorTools.Util;   // bring TransformPathExt (GetPath) into scope

#if TMP_PRESENT
using TMPro;
#endif

namespace AQ.EditorTools.Content
{
    public static class OverlayTidyLayout
    {
        [MenuItem("AQ/Content/Overlay: Tidy (dedupe + layout)")]
        public static void Tidy()
        {
            var root = FindOverlayRoot();
            if (!root)
            {
                Debug.LogWarning("[OverlayTidy] ResolutionRoot not found (searched inactive too).");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root, "Overlay Tidy");

            var rootRT = root.GetComponent<RectTransform>() ?? root.AddComponent<RectTransform>();
            StretchFull(rootRT);

            // Scaler for predictable sizing
            var scaler = root.GetComponent<CanvasScaler>() ?? root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            // Scrim
            var rootImg = root.GetComponent<Image>();
            if (rootImg) { rootImg.color = new Color32(20, 26, 40, 210); rootImg.raycastTarget = true; }

            // Panel
            var panelTr = root.transform.Find("ResolutionPanel") as RectTransform;
            if (!panelTr)
            {
                Debug.LogWarning("[OverlayTidy] 'ResolutionPanel' not found under ResolutionRoot.");
                return;
            }

            // Remove layout components that were collapsing width
            var vlg = panelTr.GetComponent<VerticalLayoutGroup>();
            if (vlg) Object.DestroyImmediate(vlg, true);
            var csf = panelTr.GetComponent<ContentSizeFitter>();
            if (csf) Object.DestroyImmediate(csf, true);

            StretchWithMargins(panelTr, left: 48, right: 48, top: 540, bottom: 420);

            var panelImg = panelTr.GetComponent<Image>();
            if (panelImg) panelImg.color = new Color32(45, 35, 60, 240);

            // Remove duplicates anywhere under ResolutionRoot; keep the ones directly on ResolutionPanel
            string[] names = { "TitleText", "BodyText", "Quest_0", "Quest_1", "Quest_2" };
            foreach (var n in names)
            {
                var keep = panelTr.Find(n);
                foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                {
                    if (!t) continue;
                    if (t == keep) continue;
                    if (t.name != n) continue;

                    var go = t.gameObject;
                    // Only consider objects in the same scene and under ResolutionRoot
                    if (go && !EditorUtility.IsPersistent(go) && go.scene.IsValid())
                    {
                        // must be under ResolutionRoot
                        var p = t;
                        bool underRoot = false;
                        while (p != null)
                        {
                            if (p == root.transform) { underRoot = true; break; }
                            p = p.parent;
                        }
                        if (!underRoot) continue;

                        // If it has a Graphic, it's a duplicate UI text-ish node → delete
                        if (go.GetComponent<Graphic>() != null)
                        {
                            Debug.Log($"[OverlayTidy] Removed duplicate '{n}' at {TransformPathExt.GetPath(t)}");
                            Object.DestroyImmediate(go, true);
                        }
                    }
                }
            }

            // Text layout
            LayoutText(panelTr, "TitleText", top: 36, height: 80, size: 48, align: TextAnchor.UpperLeft, bold: true);
            LayoutText(panelTr, "BodyText", top: 300, height: 90, size: 26, align: TextAnchor.UpperLeft);

            LayoutText(panelTr, "Quest_0", top: 400, height: 50, size: 24, align: TextAnchor.UpperLeft);
            LayoutText(panelTr, "Quest_1", top: 450, height: 50, size: 24, align: TextAnchor.UpperLeft);
            LayoutText(panelTr, "Quest_2", top: 500, height: 50, size: 24, align: TextAnchor.UpperLeft);

            // Button centered, moved lower to avoid overlap
            var btnTr = panelTr.Find("ResolveButton") as RectTransform;
            if (btnTr)
            {
                btnTr.anchorMin = btnTr.anchorMax = new Vector2(0.5f, 1f);
                btnTr.pivot = new Vector2(0.5f, 1f);

                const float top = 200f; // distance from panel top
                const float w = 420f, h = 74f;
                btnTr.offsetMin = new Vector2(-w * 0.5f, -top - h);
                btnTr.offsetMax = new Vector2(+w * 0.5f, -top);

                var btnImg = btnTr.GetComponent<Image>();
                if (btnImg) btnImg.color = new Color32(240, 240, 245, 255);

                var button = btnTr.GetComponent<Button>();
                if (button)
                {
                    var colors = button.colors;
                    colors.normalColor = new Color32(240, 240, 245, 255);
                    colors.highlightedColor = new Color32(255, 255, 255, 255);
                    colors.pressedColor = new Color32(225, 225, 230, 255);
                    button.colors = colors;
                }

                var textTr = btnTr.Find("Text");
                if (textTr)
                {
                    StyleUGUIText(textTr.GetComponent<Text>(), 24, TextAnchor.MiddleCenter, Color.black);
#if TMP_PRESENT
                    StyleTMP(textTr.GetComponent<TMP_Text>(), 24, "Center", Color.black, false);
#endif
                }
            }

            Debug.Log($"[OverlayTidy] Tidy complete → {TransformPathExt.GetPath(root.transform)}");
        }

        // ---------- helpers ----------
        static GameObject FindOverlayRoot()
        {
            var go = GameObject.Find("ResolutionRoot");
            if (go) return go;

            foreach (var comp in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (!comp) continue;
                if (comp.GetType().Name == "ResolutionContinueMB")
                {
                    var g = comp.gameObject;
                    if (g && !EditorUtility.IsPersistent(g) && g.scene.IsValid()) return g;
                }
            }
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t && t.name == "ResolutionRoot")
                {
                    var g = t.gameObject;
                    if (!EditorUtility.IsPersistent(g) && g.scene.IsValid()) return g;
                }
            }
            return null;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static void StretchWithMargins(RectTransform rt, float left, float right, float top, float bottom)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        static void LayoutText(Transform parent, string childName, float top, float height, int size, TextAnchor align, bool bold = false)
        {
            var tr = parent.Find(childName) as RectTransform;
            if (!tr) return;

            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.offsetMin = new Vector2(36f, -top - height);
            tr.offsetMax = new Vector2(-36f, -top);

            var color = Color.white;

            var ugui = tr.GetComponent<Text>();
            if (ugui)
            {
                ugui.alignment = align;
                ugui.fontSize = size;
                ugui.color = color;
                ugui.horizontalOverflow = HorizontalWrapMode.Wrap;
                ugui.verticalOverflow = VerticalWrapMode.Overflow;
                ugui.resizeTextForBestFit = false;
                ugui.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            }

#if TMP_PRESENT
            var tmp = tr.GetComponent<TMP_Text>();
            if (tmp) StyleTMP(tmp, size, AlignToTMP(align), color, bold);
#endif
        }

        static void StyleUGUIText(Text t, int size, TextAnchor align, Color color)
        {
            if (!t) return;
            t.alignment = align;
            t.fontSize = size;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = false;
        }

#if TMP_PRESENT
        static void StyleTMP(TMP_Text tmp, int size, string align, Color color, bool bold)
        {
            if (!tmp) return;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = System.Enum.TryParse<TextAlignmentOptions>(align, out var a) ? a : TextAlignmentOptions.TopLeft;
            tmp.margin = Vector4.zero;
            tmp.fontStyle = bold ? (tmp.fontStyle | FontStyles.Bold) : (tmp.fontStyle & ~FontStyles.Bold);
        }

        static string AlignToTMP(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return "TopLeft";
                case TextAnchor.UpperCenter: return "Top";
                case TextAnchor.UpperRight: return "TopRight";
                case TextAnchor.MiddleLeft: return "Left";
                case TextAnchor.MiddleCenter: return "Center";
                case TextAnchor.MiddleRight: return "Right";
                case TextAnchor.LowerLeft: return "BottomLeft";
                case TextAnchor.LowerCenter: return "Bottom";
                case TextAnchor.LowerRight: return "BottomRight";
                default: return "TopLeft";
            }
        }
#endif
    }
}
#endif

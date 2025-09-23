#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Content
{
    public static class OverlayReadableStyle
    {
        // ---------- Palette (tweak if you like) ----------
        static readonly Color32 OverlayBg   = new Color32(22, 28, 45, 220);   // deep navy overlay (no magenta)
        static readonly Color32 PanelBg     = new Color32(34, 27, 48, 238);   // dark purple card
        static readonly Color32 TitleColor  = new Color32(242, 245, 250, 255);
        static readonly Color32 BodyColor   = new Color32(220, 228, 235, 255);
        static readonly Color32 CTA_BG      = new Color32(122, 92, 255, 255);
        static readonly Color32 CTA_Text    = new Color32(11, 14, 20, 255);

        // ---------- Menus ----------
        [MenuItem("AQ/Content/Overlay: Readable Style (Auto)")]
        public static void ApplyReadableOverlayStyle_Auto()
        {
            var root = FindResolutionRoot();
            if (!root) root = CreateResolutionRoot();
            ApplyStyle(root);
        }

        [MenuItem("AQ/Content/Overlay: Style Selected")]
        public static void ApplyReadableOverlayStyle_Selected()
        {
            var go = Selection.activeGameObject;
            if (!go) { Debug.LogWarning("[OverlayStyle] Nothing selected."); return; }
            ApplyStyle(go);
        }

        [MenuItem("AQ/Content/Overlay: Create New + Style")]
        public static void CreateAndStyle()
        {
            var root = CreateResolutionRoot();
            ApplyStyle(root);
        }

        // ---------- Find / Create ----------
        static GameObject FindResolutionRoot()
        {
            var go = GameObject.Find("ResolutionRoot");
            if (go) return go;

            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t && t.name == "ResolutionRoot" && (t.hideFlags & HideFlags.HideInHierarchy) == 0)
                    return t.gameObject;
            }

            string[] variants = { "Resolution Root", "ResolutionOverlay", "OverlayRoot" };
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t && variants.Contains(t.name) && (t.hideFlags & HideFlags.HideInHierarchy) == 0)
                    return t.gameObject;
            }

            // Fall back to the top-most ScreenSpaceOverlay canvas
            var canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var candidate = canvases
                .Where(c => c && c.renderMode == RenderMode.ScreenSpaceOverlay)
                .OrderByDescending(c => c.sortingOrder)
                .FirstOrDefault();
            return candidate ? candidate.gameObject : null;
        }

        static GameObject CreateResolutionRoot()
        {
            var root = new GameObject("ResolutionRoot",
                typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster),
                typeof(Image), typeof(CanvasGroup));

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9000;

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

            var img = root.GetComponent<Image>();
            img.color = OverlayBg;
            img.raycastTarget = true;

            var panel = new GameObject("ResolutionPanel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(root.transform, false);

            var btn = new GameObject("ResolveButton", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            btn.SetParent(panel, false);

            var btnLabel = new GameObject("Text", typeof(RectTransform)).GetComponent<RectTransform>();
            btnLabel.SetParent(btn, false);

            return root;
        }

        // ---------- Styling ----------
        static void ApplyStyle(GameObject root)
        {
            if (!root) { Debug.LogWarning("[OverlayStyle] Root is null."); return; }

            // Root canvas & overlay
            var canvas = root.GetComponent<Canvas>() ?? root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9000;

            if (!root.GetComponent<GraphicRaycaster>()) root.AddComponent<GraphicRaycaster>();

            var cg = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
            cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

            var overlayImg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            overlayImg.color = OverlayBg;
            overlayImg.raycastTarget = true;

            // Panel
            var panelTr = root.transform.Find("ResolutionPanel") as RectTransform;
            if (!panelTr)
            {
                var go = new GameObject("ResolutionPanel", typeof(RectTransform), typeof(Image));
                panelTr = go.GetComponent<RectTransform>();
                panelTr.SetParent(root.transform, false);
            }
            var pImg = panelTr.GetComponent<Image>(); pImg.color = PanelBg;
            StretchHoriz(panelTr, left: 28, right: 28, centerY: -40f, height: 560f);

            // Clean duplicates from previous runs
            CleanupDuplicateTexts(panelTr);

            // Title
            var title = FindOrCreateText(panelTr, "TitleText");
            SetText(title, "Cas", 40f, TitleColor, "Center", true, Vector4.zero);
            SetRectLine(GetRT(title), y: +160, height: 60, left: 36, right: 36);

            // Continue button
            var btnTr = panelTr.Find("ResolveButton") as RectTransform;
            if (!btnTr)
            {
                var go = new GameObject("ResolveButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnTr = go.GetComponent<RectTransform>();
                btnTr.SetParent(panelTr, false);
            }
            StretchButtonCentered(btnTr, width: 340, height: 96, yFromCenter: +90);

            var btnImg = btnTr.GetComponent<Image>(); btnImg.color = CTA_BG;
            var btn = btnTr.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = CTA_BG;
            colors.highlightedColor = new Color(CTA_BG.r / 255f, CTA_BG.g / 255f, CTA_BG.b / 255f, 0.90f);
            colors.pressedColor   = new Color(CTA_BG.r / 255f, CTA_BG.g / 255f, CTA_BG.b / 255f, 0.80f);
            btn.colors = colors;

            var btnLabel = FindOrCreateText(btnTr, "Text");
            SetText(btnLabel, "Continue", 28f, CTA_Text, "Center", false, Vector4.zero);
            // Fill the button
            var btnLabelRT = GetRT(btnLabel);
            btnLabelRT.anchorMin = Vector2.zero; btnLabelRT.anchorMax = Vector2.one; btnLabelRT.pivot = new Vector2(0.5f, 0.5f);
            btnLabelRT.offsetMin = Vector2.zero; btnLabelRT.offsetMax = Vector2.zero;
            SetIfExists(btnLabel.GetType(), btnLabel, "raycastTarget", false);

            // Body
            var body = FindOrCreateText(panelTr, "BodyText");
            SetText(body, "Your investigation cracked the trail wide open.", 28f, BodyColor, "Center", true, Vector4.zero);
            SetRectLine(GetRT(body), y: +30, height: 84, left: 36, right: 36);

            // Quest lines (left-aligned, wide)
            var q0 = FindOrCreateText(panelTr, "Quest_0");
            SetText(q0, "• Investigate new lead at City Hall", 24f, BodyColor, "Left", true, Vector4.zero);
            SetRectLine(GetRT(q0), y: -40, height: 44, left: 36, right: 36);

            var q1 = FindOrCreateText(panelTr, "Quest_1");
            SetText(q1, "• Cross-check Marlow’s alibi records", 24f, BodyColor, "Left", true, Vector4.zero);
            SetRectLine(GetRT(q1), y: -86, height: 44, left: 36, right: 36);

            var q2 = FindOrCreateText(panelTr, "Quest_2");
            SetText(q2, "• Tag recovered evidence in caseboard", 24f, BodyColor, "Left", true, Vector4.zero);
            SetRectLine(GetRT(q2), y: -132, height: 44, left: 36, right: 36);

            Debug.Log("[OverlayStyle] Applied readable overlay style.");
        }

        // ---------- Cleanup ----------
        static void CleanupDuplicateTexts(RectTransform panel)
        {
            // Ensure ResolveButton has only one label named "Text"
            var btnTr = panel.Find("ResolveButton") as RectTransform;
            if (btnTr)
            {
                var label = btnTr.Find("Text");
                foreach (Transform child in btnTr)
                {
                    if (child == label) continue;
                    // remove any extra text components on other children
                    bool hasText = child.GetComponent<Text>() || GetTMP(child) != null;
                    if (hasText) Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // Remove any Quest_3+, keep only 0..2
            var toDelete = panel.Cast<Transform>()
                .Where(t => t.name.StartsWith("Quest_"))
                .Where(t =>
                {
                    var suffix = t.name.Substring(6);
                    return int.TryParse(suffix, out var i) && i > 2;
                })
                .ToArray();
            foreach (var t in toDelete) Undo.DestroyObjectImmediate(t.gameObject);
        }

        // ---------- Layout helpers ----------
        static void StretchHoriz(RectTransform rt, float left, float right, float centerY, float height)
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, -height / 2f + centerY);
            rt.offsetMax = new Vector2(-right, height / 2f + centerY);
        }

        static void SetRectLine(RectTransform rt, float y, float height, float left, float right)
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, y - height / 2f);
            rt.offsetMax = new Vector2(-right, y + height / 2f);
        }

        static void StretchButtonCentered(RectTransform rt, float width, float height, float yFromCenter)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(0f, yFromCenter);
        }

        // ---------- Text helpers (TMP if available, else UGUI) ----------
        static Component FindOrCreateText(Transform parent, string name)
        {
            var tr = parent.Find(name) as RectTransform;
            if (!tr)
            {
                var go = new GameObject(name, typeof(RectTransform));
                tr = go.GetComponent<RectTransform>();
                tr.SetParent(parent, false);
            }

            var tmpType = GetType("TMPro.TextMeshProUGUI");
            var existingTMP   = tmpType != null ? tr.GetComponent(tmpType) : null;
            var existingUGUI  = tr.GetComponent<Text>();

            if (existingTMP != null) return (Component)existingTMP;   // keep TMP if present
            if (existingUGUI != null) return existingUGUI;            // else keep UGUI

            if (tmpType != null) return (Component)tr.gameObject.AddComponent(tmpType);

            var txt = tr.gameObject.AddComponent<Text>();
            if (!txt.font) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return txt;
        }

        static RectTransform GetRT(Component c) => c ? (RectTransform)((Component)c).transform : null;

        static void SetText(Component textComp, string defaultText, float size, Color color, string align, bool wrap, Vector4 margin)
        {
            if (!textComp) return;
            var t = textComp.GetType();

            // text
            SetString(t, textComp, "text", defaultText);

            // sizes/colors
            SetNumber(t, textComp, "fontSize", size);
            SetIfExists(t, textComp, "color", color);
            SetIfExists(t, textComp, "raycastTarget", false);

            // wrapping / overflow
            var wrapProp = t.GetProperty("enableWordWrapping");
            if (wrapProp != null) wrapProp.SetValue(textComp, wrap); // TMP
            var ugui = textComp as Text;
            if (ugui)
            {
                ugui.horizontalOverflow = HorizontalWrapMode.Wrap;
                ugui.verticalOverflow   = VerticalWrapMode.Overflow;
                if (!ugui.font) ugui.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            // TMP overflowMode = Overflow
            SetEnumByName(t, textComp, "overflowMode", "Overflow");

            // alignment (TMP/UGUI)
            SetAlignment(t, textComp, align);

            // TMP margin
            var marginProp = t.GetProperty("margin");
            if (marginProp != null && margin != Vector4.zero)
                marginProp.SetValue(textComp, margin);
        }

        static void SetAlignment(Type t, object instance, string align)
        {
            var alignProp = t.GetProperty("alignment");
            if (alignProp == null) return;

            var enumType = alignProp.PropertyType;
            object enumVal = null;
            string[] candidates = align switch
            {
                "Center" => new[] { "Center", "MiddleCenter" }, // TMP / UGUI
                "Left"   => new[] { "Left", "UpperLeft", "MiddleLeft" },
                "Right"  => new[] { "Right", "UpperRight", "MiddleRight" },
                _        => new[] { align }
            };
            foreach (var c in candidates)
            {
                try { enumVal = Enum.Parse(enumType, c); break; } catch { }
            }
            if (enumVal != null) alignProp.SetValue(instance, enumVal);
        }

        static void SetEnumByName(Type t, object instance, string propName, string enumValue)
        {
            var p = t.GetProperty(propName);
            if (p == null) return;
            try
            {
                var enumType = p.PropertyType;
                var val = Enum.Parse(enumType, enumValue);
                p.SetValue(instance, val);
            }
            catch { /* ignore */ }
        }

        static void SetString(Type t, object instance, string prop, string value)
        {
            var p = t.GetProperty(prop);
            if (p == null || !p.CanWrite) return;
            var current = p.GetValue(instance) as string;
            if (string.IsNullOrWhiteSpace(current)) p.SetValue(instance, value);
        }

        static void SetNumber(Type t, object instance, string prop, float value)
        {
            var p = t.GetProperty(prop);
            if (p == null || !p.CanWrite) return;
            try
            {
                if (p.PropertyType == typeof(int)) p.SetValue(instance, Mathf.RoundToInt(value));
                else if (p.PropertyType == typeof(float)) p.SetValue(instance, value);
                else p.SetValue(instance, Convert.ChangeType(value, p.PropertyType));
            }
            catch { /* ignore */ }
        }

        static void SetIfExists(Type t, object instance, string prop, object value)
        {
            var p = t.GetProperty(prop);
            if (p == null || !p.CanWrite) return;
            try
            {
                if (value != null && p.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    p.SetValue(instance, value);
                    return;
                }
                if (p.PropertyType == typeof(int)) { p.SetValue(instance, Convert.ToInt32(value)); return; }
                if (p.PropertyType == typeof(float)) { p.SetValue(instance, Convert.ToSingle(value)); return; }
                p.SetValue(instance, value);
            }
            catch { /* ignore */ }
        }

        static Component GetTMP(Transform tr)
        {
            var tmpType = GetType("TMPro.TextMeshProUGUI");
            return tmpType != null ? tr.GetComponent(tmpType) : null;
        }

        static Type GetType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
#endif

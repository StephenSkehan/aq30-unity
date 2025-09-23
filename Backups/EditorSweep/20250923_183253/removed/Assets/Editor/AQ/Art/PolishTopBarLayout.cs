// Assets/Editor/AQ/Art/PolishTopBarLayout.cs
// AQ • Top Bar layout fixer (safe & idempotent)
// - Ensures Canvas scaler (1080x1920, match 0.5)
// - Ensures HUD_Board/TopBar exists, anchored to top (height 176)
// - Adds HorizontalLayoutGroup to TopBar (padding & spacing tuned for 1080 ref)
// - Ensures standard children exist so art-assign script can find them
//     Btn_Home, AvatarChip, EpisodeChip, Meter_Energy, Meter_Soft, Meter_Premium
// - Adds LayoutElement sizes for consistent look

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class PolishTopBarLayout
    {
        private const string LogTag = "[AQ Art]";
        private const float REF_W = 1080f;
        private const float TOPBAR_H = 176f;

        [MenuItem("AQ/Art/Fix TopBar Layout")]
        public static void Fix()
        {
            // 1) Canvas scaler (on Canvas_Board)
            var canvasTr = Find("Canvas_Board");
            if (!canvasTr)
            {
                Debug.LogWarning($"{LogTag} Canvas_Board not found in the scene.");
                return;
            }

            var canvas = canvasTr.GetComponent<Canvas>();
            if (!canvas)
            {
                canvas = canvasTr.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var scaler = canvasTr.GetComponent<CanvasScaler>()
                        ?? canvasTr.gameObject.AddComponent<CanvasScaler>(); // <-- important: AddComponent on the GameObject

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(REF_W, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (!canvasTr.GetComponent<GraphicRaycaster>())
                canvasTr.gameObject.AddComponent<GraphicRaycaster>();

            // 2) HUD_Board & TopBar path
            var hud = EnsureChild(canvasTr, "HUD_Board");
            var topBar = EnsureChild(hud, "TopBar");

            // 3) Position the TopBar at the top, height fixed
            var rt = topBar.GetComponent<RectTransform>();
            AnchorTop(rt, TOPBAR_H);

            // Ensure a background Image exists (non-blocking if you already have one)
            var bgImg = topBar.GetComponent<Image>() ?? topBar.gameObject.AddComponent<Image>();
            bgImg.color = new Color(1, 1, 1, 0f); // transparent until you assign a sprite

            // 4) Horizontal layout for neat packing
            var hlg = topBar.GetComponent<HorizontalLayoutGroup>() ?? topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.padding = new RectOffset(24, 24, 20, 20);
            hlg.spacing = 24f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 5) Ensure child nodes exist with sensible sizing (LayoutElement)
            // Order: Home | Avatar | Episode | Energy | Soft | Premium
            MakeButtonWithIcon(topBar, "Btn_Home", 96f, 120f);
            MakeAvatarChip(topBar, "AvatarChip", 120f);                    // square chip, icon sits inside
            MakeEpisodeChip(topBar, "EpisodeChip", 420f, 120f);            // pill for "Ep1-1"
            MakeMeter(topBar, "Meter_Energy", 220f, 120f);
            MakeMeter(topBar, "Meter_Soft",   220f, 120f);
            MakeMeter(topBar, "Meter_Premium",220f, 120f);

            // Let Unity know objects changed
            MarkSceneDirty(topBar.gameObject);
            Debug.Log($"{LogTag} TopBar layout polished.");
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static Transform Find(string scenePath)
        {
            // GameObject.Find supports hierarchical paths with '/'
            var go = GameObject.Find(scenePath);
            return go ? go.transform : null;
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (!child)
            {
                var go = new GameObject(name, typeof(RectTransform));
                child = go.transform;
                child.SetParent(parent, false);
                var rt = child as RectTransform;
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(100, 100);
            }
            return child;
        }

        private static void AnchorTop(RectTransform rt, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, height);
            rt.offsetMin = new Vector2(0, -height); // left/bottom
            rt.offsetMax = new Vector2(0, 0);       // right/top
        }

        private static LayoutElement EnsureLE(Transform tr, float prefW, float prefH)
        {
            var le = tr.GetComponent<LayoutElement>() ?? tr.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = prefW;
            le.preferredHeight = prefH;
            le.minHeight = prefH;
            return le;
        }

        private static Image EnsureImage(Transform tr)
        {
            return tr.GetComponent<Image>() ?? tr.gameObject.AddComponent<Image>();
        }

        private static TMP_Text EnsureLabel(Transform tr, string name, int fontSize, TextAlignmentOptions align)
        {
            var t = tr.Find(name);
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform));
                t = go.transform;
                t.SetParent(tr, false);
            }

            var tmp = t.GetComponent<TextMeshProUGUI>() ?? t.gameObject.AddComponent<TextMeshProUGUI>();
            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            tmp.text = tmp.text is { Length: > 0 } ? tmp.text : "Ep1-1";
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Transform EnsureChildImage(Transform parent, string name)
        {
            var c = parent.Find(name);
            if (!c)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image));
                c = go.transform;
                c.SetParent(parent, false);
            }
            return c;
        }

        private static void MakeButtonWithIcon(Transform topBar, string name, float w, float h)
        {
            var node = EnsureChild(topBar, name);
            EnsureLE(node, w, h);
            var img = EnsureImage(node);
            img.type = Image.Type.Sliced; // for optional chip bg
            // Place icon child (Img_Icon)
            var icon = EnsureChildImage(node, "Img_Icon");
            var rt = icon.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(h * 0.6f, h * 0.6f);
        }

        private static void MakeAvatarChip(Transform topBar, string name, float side)
        {
            var node = EnsureChild(topBar, name);
            EnsureLE(node, side, side);
            var img = EnsureImage(node);
            img.type = Image.Type.Sliced;

            // Child: Img_Avatar (fills)
            var portrait = EnsureChildImage(node, "Img_Avatar");
            var rt = portrait.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(12, 12);
            rt.offsetMax = new Vector2(-12, -12);
            portrait.GetComponent<Image>().preserveAspect = true;
        }

        private static void MakeEpisodeChip(Transform topBar, string name, float w, float h)
        {
            var node = EnsureChild(topBar, name);
            EnsureLE(node, w, h);
            var img = EnsureImage(node);
            img.type = Image.Type.Sliced;

            var label = EnsureLabel(node, "Txt_Episode", 48, TextAlignmentOptions.Center);
            label.text = string.IsNullOrEmpty(label.text) ? "Ep1-1" : label.text;
        }

        private static void MakeMeter(Transform topBar, string name, float w, float h)
        {
            var node = EnsureChild(topBar, name);
            EnsureLE(node, w, h);
            var img = EnsureImage(node);
            img.type = Image.Type.Sliced;

            // Icon
            var icon = EnsureChildImage(node, "Img_Icon");
            var rtI = icon.GetComponent<RectTransform>();
            rtI.anchorMin = new Vector2(0, 0.5f);
            rtI.anchorMax = new Vector2(0, 0.5f);
            rtI.pivot = new Vector2(0, 0.5f);
            rtI.anchoredPosition = new Vector2(18, 0);
            rtI.sizeDelta = new Vector2(h * 0.55f, h * 0.55f);

            // Value text
            var value = EnsureLabel(node, "Txt_Value", 48, TextAlignmentOptions.MidlineLeft);
            var rtV = value.rectTransform;
            rtV.anchorMin = new Vector2(0, 0);
            rtV.anchorMax = new Vector2(1, 1);
            rtV.offsetMin = new Vector2(h * 0.55f + 36f, 0);
            rtV.offsetMax = new Vector2(-16f, 0);

            // Optional timer text (hidden by default)
            var timer = EnsureLabel(node, "Txt_Timer", 28, TextAlignmentOptions.MidlineRight);
            timer.text = "01:04";
            timer.color = new Color(1f, 1f, 1f, 0.85f);
            timer.gameObject.SetActive(false);
        }

        private static void MarkSceneDirty(Object obj)
        {
#if UNITY_2021_3_OR_NEWER
            EditorUtility.SetDirty(obj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#else
            EditorUtility.SetDirty(obj);
#endif
        }
    }
}
#endif

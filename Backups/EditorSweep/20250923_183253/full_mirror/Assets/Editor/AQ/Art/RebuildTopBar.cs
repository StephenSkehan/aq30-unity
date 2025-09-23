// Assets/Editor/AQ/Art/RebuildTopBar.cs
// Deterministically (re)builds the TopBar hierarchy and applies sprites/layout.
// NOTE: This file should NOT contain FixCanvasBoardLayout. That class lives in FixCanvasBoardLayout.cs

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class RebuildTopBar
    {
        const string TAG = "[AQ Art]";
        const float TOPBAR_H = 176f;

        // Sprites live here
        const string UI_PATH = "Assets/Art/UI/TopBar/";

        [MenuItem("AQ/Art/Rebuild TopBar (deterministic)")]
        public static void Run()
        {
            // Find/ensure TopBar under Canvas_Board/HUD_Board
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas) { Debug.LogWarning($"{TAG} Canvas_Board not found."); return; }

            var hud = canvas.transform.Find("HUD_Board");
            if (!hud)
            {
                var go = new GameObject("HUD_Board", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                hud = go.transform;
            }

            var top = hud.Find("TopBar");
            if (!top)
            {
                var go = new GameObject("TopBar", typeof(RectTransform));
                go.transform.SetParent(hud, false);
                top = go.transform;
            }

            var rtTop = (RectTransform)top;
            // Anchor at top; fixed height
            rtTop.anchorMin = new Vector2(0f, 1f);
            rtTop.anchorMax = new Vector2(0f, 1f);
            rtTop.pivot     = new Vector2(0.5f, 1f);
            rtTop.sizeDelta = new Vector2(1080f, TOPBAR_H);
            rtTop.anchoredPosition = new Vector2(540f, 0f);

            // Clean children
            for (int i = top.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(top.GetChild(i).gameObject);

            // Layout
            var h = top.GetComponent<HorizontalLayoutGroup>();
            if (!h) h = top.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(24, 24, 12, 12);
            h.spacing = 24f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = false;
            h.childControlHeight = false;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            // Build items
            BuildHomeButton(top);
            BuildAvatarChip(top);
            BuildEpisodeChip(top);
            BuildMeter(top, "Meter_Energy",  "ui_top_energy.png",  showTimer: true,  sampleValue: "23",  sampleTimer: "01:04");
            BuildMeter(top, "Meter_Soft",    "ui_top_soft.png",    showTimer: false, sampleValue: "1482");
            BuildMeter(top, "Meter_Premium", "ui_top_premium.png", showTimer: false, sampleValue: "56");

            Debug.Log($"{TAG} TopBar rebuilt.");
        }

        // ---------- Builders ----------

        static void BuildHomeButton(Transform parent)
        {
            var root = Create(parent, "Btn_Home", new Vector2(96, 96));
            root.gameObject.AddComponent<Button>();
            var le = GetOrAdd<LayoutElement>(root.gameObject);
            le.preferredWidth = 96; le.preferredHeight = 96; le.minHeight = 96;

            // Backplate (9-slice)
            var back = Create(root, "Img_Back", new Vector2(96, 96));
            var backImg = AddImage(back, Load("ui_home_bg_9s.png"), true);
            backImg.type = Image.Type.Sliced;

            // Icon
            var icon = Create(root, "Img_Icon", new Vector2(72, 72));
            var iconImg = AddImage(icon, Load("ui_home.png"), true);
            iconImg.preserveAspect = true;
        }

        static void BuildAvatarChip(Transform parent)
        {
            var chip = Create(parent, "AvatarChip", new Vector2(128, 128));
            var le = GetOrAdd<LayoutElement>(chip.gameObject);
            le.preferredWidth = 128; le.preferredHeight = 128; le.minHeight = 128;

            // Portrait
            var avatar = Create(chip, "Img_Avatar", new Vector2(128, 128));
            var aImg = AddImage(avatar, Load("ui_top_avatar_portrait_02.png"), true);
            aImg.preserveAspect = true;

            // Frame (9-slice)
            var frame = Create(chip, "Img_Frame", new Vector2(128, 128));
            var fImg = AddImage(frame, Load("ui_top_avatar_frame.png"), true);
            fImg.type = Image.Type.Sliced;
        }

        static void BuildEpisodeChip(Transform parent)
        {
            var chip = Create(parent, "EpisodeChip", new Vector2(340, 96));
            var le = GetOrAdd<LayoutElement>(chip.gameObject);
            le.preferredWidth = 340; le.preferredHeight = 96; le.minHeight = 96;

            var bg = Create(chip, "Img_BG", new Vector2(340, 96));
            var bgImg = AddImage(bg, Load("ui_meter_pill_9s.png"), true);
            bgImg.type = Image.Type.Sliced;

            var txtGO = Create(chip, "Txt_Episode", new Vector2(340, 96));
            var tmp = GetOrAdd<TextMeshProUGUI>(txtGO.gameObject);
            tmp.text = "Ep1-1";
            tmp.fontSize = 48;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
        }

        static void BuildMeter(Transform parent, string name, string iconFile, bool showTimer, string sampleValue, string sampleTimer = "")
        {
            var root = Create(parent, name, new Vector2(100, 100));
            var le = GetOrAdd<LayoutElement>(root.gameObject);
            le.flexibleWidth = 0; le.flexibleHeight = 0;

            var h = GetOrAdd<HorizontalLayoutGroup>(root.gameObject);
            h.spacing = 8; h.padding = new RectOffset(0,0,0,0);
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = false;
            h.childControlHeight = false;
            h.childForceExpandWidth = true; // allow text to push a bit
            h.childForceExpandHeight = true;

            var icon = Create(root, "Img_Icon", new Vector2(96, 96));
            var iconImg = AddImage(icon, Load(iconFile), true);
            iconImg.preserveAspect = true;

            var block = Create(root, "Txt_Block", new Vector2(160, 96));
            var vb = GetOrAdd<VerticalLayoutGroup>(block.gameObject);
            vb.spacing = 0; vb.padding = new RectOffset(0,0,0,0);
            vb.childControlWidth = false; vb.childControlHeight = false;
            vb.childForceExpandWidth = false; vb.childForceExpandHeight = false;
            var blockRT = (RectTransform)block;
            blockRT.anchorMin = blockRT.anchorMax = new Vector2(0.5f, 0.5f);

            var v = Create(block, "Txt_Value", new Vector2(200, 50));
            var vtmp = GetOrAdd<TextMeshProUGUI>(v.gameObject);
            vtmp.text = sampleValue;
            vtmp.fontSize = 46;
            vtmp.alignment = TextAlignmentOptions.Center;
            vtmp.textWrappingMode = TextWrappingModes.NoWrap;
            vtmp.raycastTarget = false;

            if (showTimer)
            {
                var t = Create(block, "Txt_Timer", new Vector2(200, 50));
                var ttmp = GetOrAdd<TextMeshProUGUI>(t.gameObject);
                ttmp.text = sampleTimer;
                ttmp.fontSize = 28;
                ttmp.alignment = TextAlignmentOptions.Center;
                ttmp.textWrappingMode = TextWrappingModes.NoWrap;
                ttmp.raycastTarget = false;
            }
        }

        // ---------- helpers ----------

        static RectTransform Create(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return rt;
        }

        static Image AddImage(Transform t, Sprite s, bool center)
        {
            var img = GetOrAdd<Image>(t.gameObject);
            img.sprite = s;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            if (center)
            {
                var rt = (RectTransform)t;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
            return img;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c ? c : go.AddComponent<T>();
        }

        static Sprite Load(string fileName)
        {
            var path = UI_PATH + fileName;
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!s) Debug.LogWarning($"{TAG} Sprite missing: {path}");
            return s;
        }
    }
}
#endif

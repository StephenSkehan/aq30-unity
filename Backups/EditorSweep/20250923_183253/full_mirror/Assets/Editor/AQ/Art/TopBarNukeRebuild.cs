// ---------------------------------------------------------------------------------------------------------------------
// AQ.EditorTools.Art.TopBarNukeRebuild (balanced v4: fix heights + requested sizes)
// ---------------------------------------------------------------------------------------------------------------------

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class TopBarNukeRebuild
    {
        // Canvas
        private const int CanvasW = 1080;
        private const int CanvasH = 1920;

        // Top bar
        private const int TopBarHeight = 176;
        private const int PadLR = 24;
        private const int PadTB = 12;
        private const int Spacing = 24;

        // Left cluster
        private const int HomeSize   = 96;
        private const int HomeBack   = 112;   // requested: Img_Back = 112x112
        private const int HomeIcon   = 72;

        private const int AvatarSize = 128;   // frame
        private const int AvatarImg  = 96;    // requested: Img_Avatar = 96x96 (fits within frame)

        // Episode chip
        private const int EpisodeW   = 64;
        private const int EpisodeH   = 28;
        private const int EpisodeTextSize = 22;

        // Meters
        private const int MeterIcon        = 56; // all meters use 56
        private const int MeterGap         = 6;
        private const int MeterBlockW      = 160;
        private const int MeterH           = 56; // pin meter height

        // Text
        private const int ValueSize = 40;
        private const int TimerSize = 22;
        private const int LabelSize = 36;

        // Sprites
        private const string Pfx          = "Assets/Art/UI/TopBar/";
        private const string SPR_HOME_BG  = Pfx + "ui_home_bg_9s.png";
        private const string SPR_HOME     = Pfx + "ui_home.png";
        private const string SPR_AVATAR   = Pfx + "ui_top_avatar_portrait_02.png";
        private const string SPR_FRAME    = Pfx + "ui_top_avatar_frame.png";
        private const string SPR_PILL     = Pfx + "ui_meter_pill_9s.png";
        private const string SPR_ENERGY   = Pfx + "ui_top_energy.png";
        private const string SPR_SOFT     = Pfx + "ui_top_soft.png";
        private const string SPR_PREMIUM  = Pfx + "ui_top_premium.png";

        [MenuItem("AQ/Art/💥 Nuclear Rebuild TopBar")]
        public static void Run()
        {
            // Ensure Canvas
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas)
            {
                canvas = new GameObject("Canvas_Board", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvas.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(CanvasW, CanvasH);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // HUD root
            var hud = FindOrCreate(canvas.transform, "HUD_Board");
            Stretch(hud, AnchorType.StretchFull);

            var vlg = Ensure<VerticalLayoutGroup>(hud);
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Remove old TopBar if any
            var oldTop = hud.Find("TopBar");
            if (oldTop) Object.DestroyImmediate(oldTop.gameObject);

            // TopBar
            var topBar = new GameObject("TopBar", typeof(RectTransform)).transform;
            topBar.SetParent(hud, false);
            Stretch(topBar, AnchorType.StretchTop);
            topBar.SetSiblingIndex(0);

            var topLE = Ensure<LayoutElement>(topBar);
            topLE.preferredHeight = TopBarHeight;
            topLE.minHeight = TopBarHeight;

            var hlg = Ensure<HorizontalLayoutGroup>(topBar);
            hlg.padding = new RectOffset(PadLR, PadLR, PadTB, PadTB);
            hlg.spacing = Spacing;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // IMPORTANT: HLG should NOT control child height (prevents the 1251px blow-up)
            hlg.childControlWidth  = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            // Home
            var home = MakeSquare(topBar, "Btn_Home", HomeSize);
            PinSize(home, HomeSize, HomeSize);

            var homeBack = MakeSquare(home, "Img_Back", HomeBack); // 112x112
            ImageSet(homeBack, LoadSprite(SPR_HOME_BG), Image.Type.Sliced, false);

            var homeIcon = MakeSquare(home, "Img_Icon", HomeIcon);
            ImageSet(homeIcon, LoadSprite(SPR_HOME), Image.Type.Simple, true);
            Ensure<Button>(home.gameObject);

            // Avatar
            var avatarRoot = MakeSquare(topBar, "AvatarChip", AvatarSize);
            PinSize(avatarRoot, AvatarSize, AvatarSize);

            Ensure<RectMask2D>(avatarRoot.gameObject);
            var avatarImg = MakeSquare(avatarRoot, "Img_Avatar", AvatarImg); // 96x96
            ImageSet(avatarImg, LoadSprite(SPR_AVATAR), Image.Type.Simple, true);
            avatarImg.anchoredPosition = Vector2.zero;

            var frame = MakeSquare(avatarRoot, "Img_Frame", AvatarSize);
            ImageSet(frame, LoadSprite(SPR_FRAME), Image.Type.Sliced, false);

            // Episode chip (kept small and readable)
            var ep = MakeRect(avatarRoot, "EpisodeChip", EpisodeW, EpisodeH);
            AnchorBottomRight(ep, new Vector2(-6f, 6f));
            var epBg  = MakeRect(ep, "Img_BG", EpisodeW, EpisodeH);
            ImageSet(epBg, LoadSprite(SPR_PILL), Image.Type.Sliced, false);
            var epTxt = Ensure<TextMeshProUGUI>(MakeRect(ep, "Txt_Episode", EpisodeW, EpisodeH).gameObject);
            StyleTMP(epTxt, "Ep1-1", EpisodeTextSize, TextAlignmentOptions.Center, new Color(0.18f, 0.35f, 0.40f, 1f));

            // Flex spacer to push meters right
            var spacer = MakeRect(topBar, "Spacer_Flex", 0, 0);
            var spacerLE = Ensure<LayoutElement>(spacer);
            spacerLE.flexibleWidth = 1f;
            spacerLE.minWidth = 0;

            // Meters (pin each to exact width/height)
            BuildMeter(topBar, "Meter_Energy",  SPR_ENERGY,  "23",   "01:04", MeterIcon, 56);
            BuildMeter(topBar, "Meter_Soft",    SPR_SOFT,    "1482", null,    MeterIcon, 56);
            // gem: fill 56x56 square so its transparent padding doesn’t make it look tiny
            BuildMeter(topBar, "Meter_Premium", SPR_PREMIUM, "56",   null,    MeterIcon, 56, ignoreAspect:true);

            // StatusRow (ensure exists and placed just under TopBar)
            var status = hud.Find("StatusRow");
            if (!status)
            {
                status = new GameObject("StatusRow", typeof(RectTransform)).transform;
                status.SetParent(hud, false);
                Stretch(status, AnchorType.StretchTop);

                var sHLG = Ensure<HorizontalLayoutGroup>(status);
                sHLG.padding = new RectOffset(0, 0, 0, 0);
                sHLG.spacing = 24;
                sHLG.childAlignment = TextAnchor.UpperLeft;
                sHLG.childControlWidth  = true;
                sHLG.childControlHeight = false;
                sHLG.childForceExpandWidth  = false;
                sHLG.childForceExpandHeight = false;

                MakeStat(status, "Text_Solved", "Solved 0%");
                MakeStat(status, "Text_Evidence", "Evidence 0");
                MakeStat(status, "Text_Leads", "Leads 0");
                MakeStat(status, "Text_LastBreakthrough", "Last OK — —");

                var statusLE = Ensure<LayoutElement>(status);
                statusLE.preferredHeight = 96;
                statusLE.minHeight = 96;
            }
            status.SetSiblingIndex(1);

            Debug.Log("[AQ Art] TopBar nuked + rebuilt (balanced v4).");
        }

        // ---------- Meters

        private static void BuildMeter(Transform parent, string name, string spritePath, string value, string timer,
                                       int iconSize, int meterHeight, bool ignoreAspect=false)
        {
            var meter = MakeRect(parent, name, 0, 0);

            int totalW = iconSize + MeterGap + MeterBlockW;

            var le = Ensure<LayoutElement>(meter);
            le.preferredWidth = le.minWidth = totalW;
            le.preferredHeight = le.minHeight = meterHeight;

            var h = Ensure<HorizontalLayoutGroup>(meter);
            h.spacing = MeterGap;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth  = true;
            h.childControlHeight = false;   // DO NOT let child heights stretch
            h.childForceExpandWidth  = false;
            h.childForceExpandHeight = false;

            // icon (pin height)
            var icon = MakeSquare(meter, "Img_Icon", iconSize);
            PinSize(icon, iconSize, meterHeight);
            ImageSet(icon, LoadSprite(spritePath), Image.Type.Simple, preserveAspect: !ignoreAspect);
            var iconLE = Ensure<LayoutElement>(icon);
            iconLE.preferredHeight = iconLE.minHeight = meterHeight;

            // block (pin height)
            var block = MakeRect(meter, "Txt_Block", MeterBlockW, meterHeight);
            PinSize(block, MeterBlockW, meterHeight);
            var blockLE = Ensure<LayoutElement>(block);
            blockLE.preferredHeight = blockLE.minHeight = meterHeight;

            var v = Ensure<VerticalLayoutGroup>(block);
            v.spacing = 0;
            v.childAlignment = TextAnchor.MiddleLeft;
            v.childControlWidth  = true;
            v.childControlHeight = false;
            v.childForceExpandWidth  = false;
            v.childForceExpandHeight = false;

            var txtValue = Ensure<TextMeshProUGUI>(MakeRect(block, "Txt_Value", MeterBlockW, meterHeight/2).gameObject);
            StyleTMP(txtValue, value, ValueSize, TextAlignmentOptions.Left, Color.white);
            var valLE = Ensure<LayoutElement>(txtValue.gameObject);
            valLE.preferredHeight = valLE.minHeight = meterHeight/2;

            if (!string.IsNullOrEmpty(timer))
            {
                var txtTimer = Ensure<TextMeshProUGUI>(MakeRect(block, "Txt_Timer", MeterBlockW, meterHeight/2).gameObject);
                StyleTMP(txtTimer, timer, TimerSize, TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.85f));
                var timLE = Ensure<LayoutElement>(txtTimer.gameObject);
                timLE.preferredHeight = timLE.minHeight = meterHeight/2;
            }
        }

        private static void MakeStat(Transform parent, string name, string text)
        {
            var rt = MakeRect(parent, name, 260, 72);
            var le = Ensure<LayoutElement>(rt);
            le.preferredWidth = 260;
            le.minWidth = 200;
            le.preferredHeight = le.minHeight = 72;

            var t = Ensure<TextMeshProUGUI>(rt.gameObject);
            StyleTMP(t, text, LabelSize, TextAlignmentOptions.Left, Color.white);
        }

        // ---------- Utilities

        private enum AnchorType { StretchFull, StretchTop }

        private static Transform FindOrCreate(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t) return t;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static RectTransform MakeRect(Transform parent, string name, int w, int h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        private static RectTransform MakeSquare(Transform parent, string name, int s)
            => MakeRect(parent, name, s, s);

        private static void PinSize(RectTransform rt, int w, int h)
        {
            rt.sizeDelta = new Vector2(w, h);
            var le = Ensure<LayoutElement>(rt);
            le.preferredWidth = le.minWidth = w;
            le.preferredHeight = le.minHeight = h;
        }

        private static void Stretch(Transform t, AnchorType type)
        {
            var rt = (RectTransform)t;
            switch (type)
            {
                case AnchorType.StretchFull:
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
                    break;
                case AnchorType.StretchTop:
                    rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 1);
                    rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(0, TopBarHeight);
                    break;
            }
        }

        private static void AnchorBottomRight(RectTransform rt, Vector2 offset)
        {
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(1, 0);
            rt.anchoredPosition = offset;
        }

        private static void ImageSet(RectTransform rt, Sprite s, Image.Type type, bool preserveAspect)
        {
            var img = Ensure<Image>(rt.gameObject);
            img.sprite = s;
            img.type = type;
            img.preserveAspect = preserveAspect;
        }

        private static void StyleTMP(TextMeshProUGUI t, string text, int size, TextAlignmentOptions align, Color color)
        {
            t.text = text;
            t.alignment = align;
            t.fontSize = size;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.color = color;
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c ? c : go.AddComponent<T>();
        }
        private static T Ensure<T>(Transform tr) where T : Component => Ensure<T>(tr.gameObject);

        private static Sprite LoadSprite(string path)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!tex)
            {
                Debug.LogWarning($"[AQ Art] Sprite missing: {path}");
                return null;
            }
            var spr = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (spr == null) spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            return spr;
        }
    }
}

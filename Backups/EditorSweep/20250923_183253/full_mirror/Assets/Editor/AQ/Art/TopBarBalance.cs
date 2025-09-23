// Assets/Editor/AQ/Art/TopBarBalance.cs
// One-click, deterministic clean-up for the top bar layout.
// Safe to re-run; idempotent.

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class TopBarBalance
    {
        // ----- Public entry point
        [MenuItem("AQ/Art/TopBar/Balance Layout")]
        public static void BalanceLayout()
        {
            var topBar = Find("Canvas_Board/HUD_Board/TopBar");
            if (topBar == null)
            {
                LogWarn("TopBar not found at Canvas_Board/HUD_Board/TopBar");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(topBar.gameObject, "[AQ Art] Balance TopBar Layout");

            ConfigureTopBar(topBar);
            var home = Ensure(topBar, "Btn_Home");
            ConfigureHome(home);

            var avatarChip = Ensure(topBar, "AvatarChip");
            ConfigureAvatarCluster(avatarChip);

            // spacer pushes meters to the right for balance
            var spacer = Ensure(topBar, "Spacer");
            EnsureLayoutElement(spacer, minW: -1, minH: -1, prefW: -1, prefH: -1, flexW: 1, flexH: 0);

            var meters = Ensure(topBar, "Meters");
            ConfigureMetersCluster(meters);

            // Final pass: force a layout rebuild so you see results immediately.
            ForceRebuildImmediate(topBar);
            EditorUtility.SetDirty(topBar);
            Debug.Log("[AQ Art] TopBar balanced.");
        }

        // ----- TopBar container and HLG
        private static void ConfigureTopBar(Transform topBar)
        {
            var rt = topBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 176f);
            rt.anchoredPosition = new Vector2(0f, 0f);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(topBar);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 24f;
            hlg.padding = new RectOffset(24, 24, 12, 12);
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }

        // ----- Home button
        private static void ConfigureHome(Transform home)
        {
            var rt = home.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(96f, 96f);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            EnsureLayoutElement(home, prefW: 96, prefH: 96, minW: -1, minH: 96);

            // Backplate (9-slice) + Icon
            var imgBack = Ensure(home, "Img_Back");
            var imgIcon = Ensure(home, "Img_Icon");

            SetupImage(imgBack, "Assets/Art/UI/TopBar/ui_home_bg_9s.png", Image.Type.Sliced, preserveAspect: false);
            SetupImage(imgIcon, "Assets/Art/UI/TopBar/ui_home.png", Image.Type.Simple, preserveAspect: true);

            imgBack.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 96);
            imgIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 72);
        }

        // ----- Avatar cluster: frame + avatar + episode chip overlay (bottom-right)
        private static void ConfigureAvatarCluster(Transform avatar)
        {
            var rt = avatar.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(128f, 128f);
            EnsureLayoutElement(avatar, prefW: 128, prefH: 128, minW: -1, minH: 128);

            // Ensure children
            var imgAvatar = Ensure(avatar, "Img_Avatar");
            var imgFrame  = Ensure(avatar, "Img_Frame");
            var epChip    = Ensure(avatar, "EpisodeChip");

            // Frame
            SetupImage(imgFrame, "Assets/Art/UI/TopBar/ui_top_avatar_frame.png", Image.Type.Sliced, preserveAspect: false);
            var frt = imgFrame.GetComponent<RectTransform>();
            frt.sizeDelta = new Vector2(128, 128);
            frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 0.5f);
            frt.anchoredPosition = Vector2.zero;

            // Avatar portrait – fit inside frame, preserve aspect, no overflow.
            SetupImage(imgAvatar, "Assets/Art/UI/TopBar/ui_top_avatar_portrait_02.png", Image.Type.Simple, preserveAspect: true);
            var art = imgAvatar.GetComponent<RectTransform>();
            art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f);
            // Shrink slightly so hair corners don't escape the frame ring.
            art.sizeDelta = new Vector2(112, 112);
            art.anchoredPosition = Vector2.zero;

            // Episode chip overlay (bottom-right of the avatar)
            var chipBG = Ensure(epChip, "Img_BG");
            var chipTXT = EnsureTMP(epChip, "Txt_Episode");

            SetupImage(chipBG, "Assets/Art/UI/TopBar/ui_meter_pill_9s.png", Image.Type.Sliced, preserveAspect: false);

            var crt = epChip.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(1f, 0f); // bottom-right
            crt.pivot = new Vector2(1f, 0f);
            crt.sizeDelta = new Vector2(144, 56);
            crt.anchoredPosition = new Vector2(-6f, 6f);

            var brt = chipBG.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(144, 56);
            brt.anchoredPosition = Vector2.zero;

            var tmp = chipTXT.GetComponent<TMP_Text>();
            tmp.text = "Ep1-1";
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 40;
            tmp.alignment = TextAlignmentOptions.Center;
            // dark teal for readability over the cream pill
            ColorUtility.TryParseHtmlString("#0E5459", out var teal);
            tmp.color = teal;

            var trt = chipTXT.GetComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(144, 56);
            trt.anchoredPosition = Vector2.zero;

            // Ensure chip sits above the frame graphic (but avatar below the frame)
            imgAvatar.SetSiblingIndex(0);
            imgFrame.SetSiblingIndex(1);
            epChip.SetSiblingIndex(2);
        }

        // ----- Right cluster with the three meters
        private static void ConfigureMetersCluster(Transform meters)
        {
            var hlg = GetOrAdd<HorizontalLayoutGroup>(meters);
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.spacing = 16f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Ensure three meters exist
            var mEnergy  = Ensure(meters, "Meter_Energy");
            var mSoft    = Ensure(meters, "Meter_Soft");
            var mPremium = Ensure(meters, "Meter_Premium");

            ConfigureSingleMeter(mEnergy, "Assets/Art/UI/TopBar/ui_top_energy.png",  "23",  "01:04");  // shows timer
            ConfigureSingleMeter(mSoft,   "Assets/Art/UI/TopBar/ui_top_soft.png",    "1482", null);
            ConfigureSingleMeter(mPremium,"Assets/Art/UI/TopBar/ui_top_premium.png", "56",   null);
        }

        // Each meter is a mini HLG: [icon(56)] [VGroup: value (and optional timer)]
        private static void ConfigureSingleMeter(Transform meter, string iconPath, string value, string timer)
        {
            var rt = meter.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 100); // height target; width is content-driven

            var hlg = GetOrAdd<HorizontalLayoutGroup>(meter);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // icon
            var icon = Ensure(meter, "Img_Icon");
            SetupImage(icon, iconPath, Image.Type.Simple, preserveAspect: true);
            var irt = icon.GetComponent<RectTransform>();
            irt.sizeDelta = new Vector2(56, 56);

            // text block (vertical)
            var block = Ensure(meter, "Txt_Block");
            var vlg = GetOrAdd<VerticalLayoutGroup>(block);
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.spacing = 0f;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var valueGO = EnsureTMP(block, "Txt_Value");
            var valueTMP = valueGO.GetComponent<TMP_Text>();
            valueTMP.text = value;
            valueTMP.fontSize = 46;
            valueTMP.alignment = TextAlignmentOptions.Left;
            valueTMP.enableAutoSizing = false;

            // Optional timer row (only for Energy)
            var timerGO = EnsureTMP(block, "Txt_Timer");
            var timerTMP = timerGO.GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(timer))
            {
                timerTMP.text = "";
                timerGO.gameObject.SetActive(false);
            }
            else
            {
                timerGO.gameObject.SetActive(true);
                timerTMP.text = timer;
                timerTMP.fontSize = 28;
                timerTMP.alignment = TextAlignmentOptions.Left;
            }

            // layout sizes
            var brt = block.GetComponent<RectTransform>();
            brt.sizeDelta = new Vector2(160, 56);

            // Prevent overlap by giving the block a flexible width and the meter itself no forced width.
            EnsureLayoutElement(block, prefW: 160, prefH: 56, minW: 120, minH: 40, flexW: 1, flexH: 0);
            EnsureLayoutElement(meter, prefW: -1, prefH: 100, minW: -1, minH: -1, flexW: 0, flexH: 0);
        }

        // ----- Helpers -----------------------------------------------------------------

        private static Transform Find(string path)
        {
            var go = GameObject.Find(path);
            return go != null ? go.transform : null;
        }

        private static T GetOrAdd<T>(Transform t) where T : Component
        {
            var c = t.GetComponent<T>();
            if (c == null) c = t.gameObject.AddComponent<T>();
            return c;
        }

        private static Transform Ensure(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                child = go.transform;
            }
            return child;
        }

        private static Transform EnsureTMP(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                child = go.transform;
                go.AddComponent<TextMeshProUGUI>();
            }
            if (child.GetComponent<TMP_Text>() == null) child.gameObject.AddComponent<TextMeshProUGUI>();
            return child;
        }

        private static void SetupImage(Transform t, string spritePath, Image.Type type, bool preserveAspect)
        {
            var img = GetOrAdd<Image>(t);
            img.type = type;
            img.preserveAspect = preserveAspect;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
                LogWarn($"Sprite missing: {spritePath}");
            img.sprite = sprite;
        }

        private static void EnsureLayoutElement(Transform t, float minW=-1, float minH=-1, float prefW=-1, float prefH=-1, int flexW=0, int flexH=0)
        {
            var le = GetOrAdd<LayoutElement>(t);
            le.minWidth = minW;
            le.minHeight = minH;
            le.preferredWidth = prefW;
            le.preferredHeight = prefH;
            le.flexibleWidth = flexW;
            le.flexibleHeight = flexH;
        }

        // renamed to avoid collision with UnityEngine.UI.LayoutRebuilder
        private static void ForceRebuildImmediate(Transform root)
        {
            var rt = root as RectTransform;
            if (rt != null)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private static void LogWarn(string msg) => Debug.LogWarning($"[AQ Art] {msg}");
    }
}

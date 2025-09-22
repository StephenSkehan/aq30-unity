#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class BuildTopBarScaffold
    {
        [MenuItem("AQ/Art/Build TopBar Scaffold")]
        public static void Build()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas) { Debug.LogWarning("[AQ Art] Canvas_Board not found."); return; }

            var hud = Ensure(canvas.transform, "HUD_Board");
            var top = Ensure(hud, "TopBar");

            // Ensure TopBar Rect + layout
            var rt = top.GetComponent<RectTransform>();
            if (!rt) rt = top.gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 176);

            var hlg = top.GetComponent<HorizontalLayoutGroup>() ?? top.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.padding = new RectOffset(24, 24, 12, 12);
            hlg.spacing = 24;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // --- Home button (Btn_Home)
            var home = Ensure(top, "Btn_Home");
            SetSize(home, 120, 120);
            Ensure<Button>(home);
            var homeBG = Ensure(home, "BG");           // 9-sliced chip (optional)
            SetSize(homeBG, 120, 120);
            Ensure<Image>(homeBG);
            var homeGlyph = Ensure(home, "Glyph");     // glyph image
            SetSize(homeGlyph, 88, 88);
            Ensure<Image>(homeGlyph);

            // --- Avatar chip (AvatarChip)
            var avatar = Ensure(top, "AvatarChip");
            SetSize(avatar, 120, 120);
            Ensure<Image>(avatar);                     // frame lives on parent
            var imgAvatar = Ensure(avatar, "Img_Avatar");
            SetSize(imgAvatar, 92, 92);
            Ensure<Image>(imgAvatar);

            // --- Episode chip (EpisodeChip)
            var ep = Ensure(top, "EpisodeChip");
            SetSize(ep, 420, 120);
            Ensure<Image>(ep);                         // 9-sliced pill
            var epText = Ensure(ep, "Txt_Episode");
            SetSize(epText, 360, 80);
            var tmpEp = epText.GetComponent<TextMeshProUGUI>() ?? epText.gameObject.AddComponent<TextMeshProUGUI>();
            tmpEp.text = "Ep1-1";
            tmpEp.fontSize = 48;
            tmpEp.alignment = TextAlignmentOptions.Midline;

            // --- Meter: Energy
            BuildMeter(top, "Meter_Energy", includeTimer:true);

            // --- Meter: Soft
            BuildMeter(top, "Meter_Soft", includeTimer:false);

            // --- Meter: Premium
            BuildMeter(top, "Meter_Premium", includeTimer:false);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[AQ Art] TopBar scaffold built (Btn_Home, AvatarChip, EpisodeChip, Meter_Energy, Meter_Soft, Meter_Premium).");
        }

        // ---------- Helpers ----------
        static Transform Ensure(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform));
                t = go.transform;
                t.SetParent(parent, false);
            }
            return t;
        }

        static T Ensure<T>(Transform t) where T : Component
        {
            var c = t.GetComponent<T>();
            if (!c) c = t.gameObject.AddComponent<T>();
            return c;
        }

        static void SetSize(Transform t, float w, float h)
        {
            var rt = t as RectTransform;
            if (!rt) return;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
        }

        static void BuildMeter(Transform top, string meterName, bool includeTimer)
        {
            var m = Ensure(top, meterName);
            SetSize(m, 240, 120);
            var layout = m.GetComponent<HorizontalLayoutGroup>() ?? m.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 12;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            var icon = Ensure(m, "Img_Icon");
            SetSize(icon, 96, 96);
            Ensure<Image>(icon);

            var valueGO = Ensure(m, "Txt_Value");
            SetSize(valueGO, 100, 80);
            var tmpV = valueGO.GetComponent<TextMeshProUGUI>() ?? valueGO.gameObject.AddComponent<TextMeshProUGUI>();
            tmpV.text = "0";
            tmpV.fontSize = 48;
            tmpV.alignment = TextAlignmentOptions.Midline;

            if (includeTimer)
            {
                var timer = Ensure(m, "Txt_Timer");
                SetSize(timer, 100, 40);
                var tmpT = timer.GetComponent<TextMeshProUGUI>() ?? timer.gameObject.AddComponent<TextMeshProUGUI>();
                tmpT.text = "01:00";
                tmpT.fontSize = 28;
                tmpT.alignment = TextAlignmentOptions.Midline;
                tmpT.color = new Color(1f, 1f, 1f, 0.85f);
            }

            var le = m.GetComponent<LayoutElement>() ?? m.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 240; le.preferredHeight = 120;
        }
    }
}
#endif

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;
using AQ.App.UI.Settings;
using AQ.UI.Hints;

namespace AQ.UI.Settings
{
    /// <summary>Settings > Config tab (Stephen-ruled 2026-08-14): player-facing
    /// switches. First resident: hints on/off, default on.</summary>
    public static class ConfigSettingsTab
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
            => GameControlPanelMB.RegisterExternalTab("Config", Build, order: 1);

        private static void Build(RectTransform content)
        {
            var row = new GameObject("HintsRow", typeof(RectTransform));
            row.transform.SetParent(content, false);
            var rt       = (RectTransform)row.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(40f, -130f);
            rt.offsetMax = new Vector2(-40f, -30f);

            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(rt, false);
            var lrt       = (RectTransform)label.transform;
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(0.6f, 1f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var ltmp           = label.AddComponent<TextMeshProUGUI>();
            ltmp.text          = "Gameplay hints";
            ltmp.fontSize      = 34f;
            ltmp.color         = AQTheme.Paper;
            ltmp.alignment     = TextAlignmentOptions.MidlineLeft;
            ltmp.raycastTarget = false;
            AQTheme.StyleText(ltmp);

            var btnGo = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(rt, false);
            var brt       = (RectTransform)btnGo.transform;
            brt.anchorMin = new Vector2(0.68f, 0.1f);
            brt.anchorMax = new Vector2(1f, 0.9f);
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var img = btnGo.GetComponent<Image>();

            var txtGo = new GameObject("T", typeof(RectTransform));
            txtGo.transform.SetParent(brt, false);
            var trt       = (RectTransform)txtGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var ttmp           = txtGo.AddComponent<TextMeshProUGUI>();
            ttmp.fontSize      = 32f;
            ttmp.color         = AQTheme.Paper;
            ttmp.alignment     = TextAlignmentOptions.Center;
            ttmp.raycastTarget = false;
            AQTheme.StyleText(ttmp, display: true);

            void Paint()
            {
                bool on = HintService.Enabled;
                ttmp.text = on ? "ON" : "OFF";
                AQTheme.StyleButton(img, on ? new Color(0.18f, 0.42f, 0.28f, 1f)
                                            : new Color(0.35f, 0.33f, 0.30f, 1f));
            }

            btnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                HintService.Enabled = !HintService.Enabled;
                Paint();
            });
            Paint();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using AQ.App.Services.Ads;
using AQ.App.Services.Purchasing;
using AQ.App.UI.Common;
using AQ.App.UI.Settings;

namespace AQ.App.UI.Settings
{
    /// <summary>
    /// "Privacy" tab in the settings panel: Restore Purchases, privacy policy
    /// link, and (EEA users only) the UMP privacy-options form.
    /// Lives in Assembly-CSharp so it can reach Purchase/Consent services.
    /// </summary>
    public static class PrivacySettingsTab
    {
        private const string PolicyUrl = "https://indigochimpstudios.com/privacy";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            GameControlPanelMB.RegisterExternalTab("Privacy", Build);
        }

        private static void Build(RectTransform parent)
        {
            AddButton(parent, "Restore Purchases", 0.80f, () =>
            {
                PurchaseService.Instance?.RestorePurchases();
                ToastService.Show("restore", "Checking previous purchases…", 2f);
            });

            AddButton(parent, "Privacy Policy", 0.56f, () => Application.OpenURL(PolicyUrl));

            // Only meaningful where UMP requires a revisit option (EEA/UK).
            if (ConsentService.PrivacyOptionsRequired)
            {
                AddButton(parent, "Privacy & Ad Consent", 0.32f, () =>
                    ConsentService.ShowPrivacyOptions(error =>
                    {
                        if (!string.IsNullOrEmpty(error))
                            ToastService.Show("consent_err", "Could not open consent options.", 2f);
                    }));
            }

            AddVersionLabel(parent);
        }

        private static void AddButton(RectTransform parent, string label, float anchorTop, System.Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt        = (RectTransform)go.transform;
            rt.anchorMin  = new Vector2(0.10f, anchorTop - 0.16f);
            rt.anchorMax  = new Vector2(0.90f, anchorTop);
            rt.sizeDelta  = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.22f, 0.22f, 0.28f, 1f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            var lblRt       = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            var txt       = lblGo.AddComponent<Text>();
            txt.text      = label;
            txt.fontSize  = 30;
            txt.color     = Color.white;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
        }

        private static void AddVersionLabel(RectTransform parent)
        {
            var go = new GameObject("Version", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt        = (RectTransform)go.transform;
            rt.anchorMin  = new Vector2(0.10f, 0.02f);
            rt.anchorMax  = new Vector2(0.90f, 0.10f);
            rt.sizeDelta  = Vector2.zero;
            var txt       = go.AddComponent<Text>();
            txt.text      = $"Ally Quinn: True Crime Merge  v{Application.version}";
            txt.fontSize  = 22;
            txt.color     = new Color(0.55f, 0.55f, 0.55f, 1f);
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
        }
    }
}

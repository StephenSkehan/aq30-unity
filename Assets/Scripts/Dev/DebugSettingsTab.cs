#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App.CaseFlow;
using AQ.App.Economy;
using AQ.App.UI;
using AQ.App.UI.Board;
using AQ.App.UI.Settings;
using AQ.SharedKernel.Economy;

namespace AQ.Dev
{
    /// <summary>
    /// Settings > Debug tab (dev builds only): the live status line (episode /
    /// step / leads) and the RESET / -50 ENERGY / CRASH TEST buttons, all inside
    /// the panel. 2026-07-21 ruling — replaces the old DEBUG INFO toggle, the
    /// OnGUI overlay and the floating DevTestPanel buttons. Whole file compiles
    /// out of release builds. Lives in Assembly-CSharp because RESET needs
    /// BoardSaveSystem.ClearSave.
    /// </summary>
    public static class DebugSettingsTab
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
            => GameControlPanelMB.RegisterExternalTab("Debug", Build, order: -1); // before Privacy

        private static void Build(RectTransform ct)
        {
            // Live status line — refreshes while the tab is visible.
            var status = MakeText("StatusLine", ct, CaseFlowDebugOverlayMB.BuildStatusLine(),
                26, AQTheme.Paper);
            var srt = status.rectTransform;
            srt.anchorMin = new Vector2(0.07f, 0.85f);
            srt.anchorMax = new Vector2(0.93f, 0.98f);
            srt.sizeDelta = Vector2.zero;
            ct.gameObject.AddComponent<StatusLineRefresher>().Target = status;

            MakeActionButton(ct, "RESET",      0, new Color(0.55f, 0.20f, 0.20f), ResetToStart);
            MakeActionButton(ct, "-50 ENERGY", 1, new Color(0.20f, 0.35f, 0.55f), DrainEnergy);
            MakeActionButton(ct, "CRASH TEST", 2, new Color(0.45f, 0.30f, 0.10f), CrashTest);

            var note = MakeText("Note", ct,
                "Dev builds only — this tab does not exist in the production release.",
                22, AQTheme.PaperDim);
            var nrt = note.rectTransform;
            nrt.anchorMin = new Vector2(0.07f, 0.02f);
            nrt.anchorMax = new Vector2(0.93f, 0.13f);
            nrt.sizeDelta = Vector2.zero;
        }

        // Runs only while the Debug tab's content object is active (tab selected).
        private sealed class StatusLineRefresher : MonoBehaviour
        {
            public TextMeshProUGUI Target;
            private float _nextAt;

            void OnEnable() => _nextAt = 0f;

            void Update()
            {
                if (Target == null || Time.unscaledTime < _nextAt) return;
                _nextAt = Time.unscaledTime + 0.25f;
                Target.text = CaseFlowDebugOverlayMB.BuildStatusLine();
            }
        }

        // ---- actions (moved verbatim from the retired DevTestPanelMB) ----

        private static void ResetToStart()
        {
            var wallet = WalletLocator.Instance;
            if (wallet != null)
            {
                wallet.TrySpend(Currency.Soft,    wallet.Get(Currency.Soft),    "dev.reset");
                wallet.TrySpend(Currency.Premium, wallet.Get(Currency.Premium), "dev.reset");
                wallet.TrySpend(Currency.Energy,  wallet.Get(Currency.Energy),  "dev.reset");
            }
            BoardSaveSystem.ClearSave();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // The settings panel is DontDestroyOnLoad — close it so the fresh
            // scene doesn't boot underneath an open modal.
            var panel = UnityEngine.Object.FindAnyObjectByType<GameControlPanelMB>();
            if (panel != null) panel.gameObject.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static void DrainEnergy()
        {
            var wallet = WalletLocator.Instance;
            if (wallet == null) return;
            int balance = wallet.Get(Currency.Energy);
            wallet.TrySpend(Currency.Energy, Mathf.Min(50, balance), "dev.drain");
        }

        private static bool _threwOnce;

        private static void CrashTest()
        {
            // Two crash classes, one button. First tap: managed C# exception —
            // Unity keeps running, Crashlytics records it as fatal (FirebaseBootstrap
            // sets ReportUncaughtExceptionsAsFatal). Second tap: true native crash —
            // process dies. Both upload on NEXT launch.
            if (!_threwOnce)
            {
                _threwOnce = true;
                throw new InvalidOperationException("Crashlytics pipeline test crash — deliberate.");
            }

            UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
        }

        // ---- UI helpers (panel look, AQTheme tokens) ----

        private static void MakeActionButton(RectTransform parent, string label, int row,
            Color color, Action onClick)
        {
            var go = new GameObject("Dev_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            float top = 0.78f - row * 0.20f;
            rt.anchorMin = new Vector2(0.18f, top - 0.15f);
            rt.anchorMax = new Vector2(0.82f, top);
            rt.sizeDelta = Vector2.zero;

            var img = go.GetComponent<Image>();
            AQTheme.Round(img, color);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var txt = MakeText("Label", rt, label, 30, Color.white);
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            var trt = txt.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI MakeText(string name, Transform parent, string text,
            int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text          = text;
            t.fontSize      = size;
            t.color         = color;
            t.alignment     = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            AQTheme.StyleText(t, display: false);
            return t;
        }
    }
}
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App.Economy;
using AQ.App.UI.Board;
using AQ.SharedKernel.Economy;

namespace AQ.App.Dev
{
    /// <summary>
    /// Dev-build-only test panel: full reset, energy drain, Crashlytics crash test.
    /// Compiled out of release builds entirely.
    /// </summary>
    public sealed class DevTestPanelMB : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (UnityEngine.Object.FindFirstObjectByType<DevTestPanelMB>() != null) return;
            var go = new GameObject("__DevTestPanel", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(go);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            // Below the dialogue canvas (order 10) so dev buttons never cover
            // character portraits during dialogue QA; still above the board.
            canvas.sortingOrder = 5;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            go.AddComponent<DevTestPanelMB>();
        }

        private void Start()
        {
            MakeButton("RESET", 0, new Color(0.55f, 0.20f, 0.20f, 0.85f), ResetToStart);
            MakeButton("-50 ENERGY", 1, new Color(0.20f, 0.35f, 0.55f, 0.85f), DrainEnergy);
            MakeButton("CRASH TEST", 2, new Color(0.45f, 0.30f, 0.10f, 0.85f), CrashTest);
        }

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

        private void MakeButton(string label, int index, Color color, Action onClick)
        {
            var go = new GameObject("Dev_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rt              = (RectTransform)go.transform;
            rt.anchorMin        = new Vector2(0f, 0.5f);
            rt.anchorMax        = new Vector2(0f, 0.5f);
            rt.pivot            = new Vector2(0f, 0.5f);
            rt.sizeDelta        = new Vector2(230f, 70f);
            rt.anchoredPosition = new Vector2(10f, 160f - index * 85f);
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            var lblRt       = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            var tmp           = lblGo.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 26f;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.color         = Color.white;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
    }
}
#endif

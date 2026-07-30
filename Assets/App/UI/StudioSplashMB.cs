using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI
{
    /// Studio splash card shown once per app run at boot; tap to skip once settled.
    public sealed class StudioSplashMB : MonoBehaviour
    {
        static bool _shownThisRun;

        // The logo PNG has an opaque near-white background; the plate uses the
        // same value so the square sits seamlessly on the card.
        static readonly Color Plate     = new Color32(252, 252, 252, 255);
        static readonly Color Indigo    = new Color32(63, 48, 116, 255);
        static readonly Color IndigoDim = new Color32(146, 137, 184, 255);

        const float ContentFadeIn = 0.35f;
        const float FadeOut       = 0.45f;
        const float SkippableAfter = 0.8f;
        static float Hold => Application.isEditor ? 0.6f : 3.9f; // keep QA loops fast

        // Cycled under the logo during the hold to read as a real boot sequence.
        static readonly string[] StatusLines =
        {
            "CHECKING CASE FILES",
            "TUNING THE TIP LINE",
            "PREPARING THE BOARD",
            "LOADING",
        };

        CanvasGroup _root, _content;
        TMP_Text _loading;
        bool _skipRequested;

        // Clamped so an editor step or a device first-frame hitch (shader
        // warmup, asset load) can't swallow the whole card in one frame.
        static float Dt => Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);

        // Statics survive play sessions when domain reload is skipped — without
        // this reset the splash silently never shows again after the first run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => _shownThisRun = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInstall()
        {
            if (_shownThisRun) return; // once per app run, not per scene load
            _shownThisRun = true;

            var logo = Resources.Load<Sprite>("App/UI/Splash/indigo_chimp_logo");
            if (logo == null)
            {
                Debug.LogWarning("[StudioSplash] indigo_chimp_logo sprite missing — splash skipped.");
                return;
            }

            var go = new GameObject("[StudioSplash]");
            DontDestroyOnLoad(go);
            go.AddComponent<StudioSplashMB>().Build(logo);
        }

        void Build(Sprite logo)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6000; // above every other overlay in the game
            gameObject.AddComponent<GraphicRaycaster>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            _root = gameObject.AddComponent<CanvasGroup>();

            // Plate is opaque from frame one so the game never flashes behind it.
            // raycastTarget keeps boot taps from reaching the game underneath;
            // skip detection itself is raw input in Update() — EventSystem routing
            // to dynamically-created overlay canvases is unreliable in Unity 6
            // (same failure class as the DialogueRunner tap regression).
            var plate = MakeRect("Plate", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var plateImg = plate.gameObject.AddComponent<Image>();
            plateImg.color = Plate;
            plateImg.raycastTarget = true;

            // Content group fades in over the plate.
            var content = MakeRect("Content", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _content = content.gameObject.AddComponent<CanvasGroup>();
            _content.alpha = 0f;
            _content.interactable = _content.blocksRaycasts = false;

            // Logo — centred, slightly above the midline.
            var logoRt = MakeRect("Logo", content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(860f, 860f), new Vector2(0f, 140f));
            var logoImg = logoRt.gameObject.AddComponent<Image>();
            logoImg.sprite = logo;
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;

            // LOADING dots under the logo.
            _loading = AddText(content, "Loading", "LOADING",
                34f, Indigo, display: true,
                new Vector2(0.5f, 0.5f), new Vector2(960f, 48f), new Vector2(0f, -440f));
            _loading.characterSpacing = 8f;

            // Footer — copyright + build version.
            AddText(content, "Copyright", "© 2026 Indigo Chimp Studios · All rights reserved",
                22f, IndigoDim, display: false,
                new Vector2(0.5f, 0f), new Vector2(900f, 32f), new Vector2(0f, 96f));
            AddText(content, "Version", $"v{Application.version}",
                18f, IndigoDim, display: false,
                new Vector2(0.5f, 0f), new Vector2(900f, 26f), new Vector2(0f, 62f));

            StartCoroutine(Run());
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                _skipRequested = true;
        }

        IEnumerator Run()
        {
            float t = 0f;

            // settle in
            while (t < ContentFadeIn)
            {
                t += Dt;
                _content.alpha = Mathf.Clamp01(t / ContentFadeIn);
                yield return null;
            }
            _content.alpha = 1f;

            // hold (tap skips once the card has settled)
            float held = 0f;
            while (held < Hold)
            {
                held += Dt;
                AnimateLoadingDots(ContentFadeIn + held);
                if (_skipRequested && ContentFadeIn + held >= SkippableAfter) break;
                yield return null;
            }

            // fade the whole card out to reveal the game
            t = 0f;
            while (t < FadeOut)
            {
                t += Dt;
                _root.alpha = 1f - Mathf.Clamp01(t / FadeOut);
                yield return null;
            }
            Destroy(gameObject);
        }

        void AnimateLoadingDots(float elapsed)
        {
            int line = Mathf.Min(Mathf.FloorToInt(elapsed / 1.1f), StatusLines.Length - 1);
            int dots = 1 + Mathf.FloorToInt(elapsed * 3f) % 3;
            _loading.text = StatusLines[line] + new string('.', dots);
        }

        // ----- helpers -----

        static RectTransform MakeRect(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        static TMP_Text AddText(RectTransform parent, string name, string text,
            float size, Color color, bool display,
            Vector2 anchor, Vector2 rectSize, Vector2 pos)
        {
            var rt = MakeRect(name, parent, anchor, anchor, rectSize, pos);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display);
            return tmp;
        }
    }
}

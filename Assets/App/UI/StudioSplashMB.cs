using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AQ.App.UI
{
    /// Studio splash card shown once per app run at boot; tap to skip once settled.
    /// On a fresh install the Ally 15s promo film plays after the logo card,
    /// before the game is revealed (FTUE only — aq.ftue.promo.seen).
    public sealed class StudioSplashMB : MonoBehaviour
    {
        static bool _shownThisRun;

        /// <summary>True while the boot overlay (logo card and/or promo film)
        /// still covers the game. FTUE choreography holds on this so the intro
        /// dialogue can't boot underneath.</summary>
        public static bool Showing { get; private set; }

        const string PromoSeenKey  = "aq.ftue.promo.seen";
        const string PromoResource = "App/Video/ally_promo_15s";

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
        Image _plateImg;
        TMP_Text _loading;
        bool _skipRequested;

        // Clamped so an editor step or a device first-frame hitch (shader
        // warmup, asset load) can't swallow the whole card in one frame.
        static float Dt => Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);

        // Statics survive play sessions when domain reload is skipped — without
        // this reset the splash silently never shows again after the first run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { _shownThisRun = false; Showing = false; }

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
            _plateImg = plate.gameObject.AddComponent<Image>();
            _plateImg.color = Plate;
            _plateImg.raycastTarget = true;

            Showing = true;

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

            // FTUE only: the Ally promo film plays between the logo and the game.
            yield return PromoStage();

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

        void OnDestroy() => Showing = false;

        IEnumerator PromoStage()
        {
            if (PlayerPrefs.GetInt(PromoSeenKey, 0) != 0) yield break;

            var clip = Resources.Load<VideoClip>(PromoResource);
            if (clip == null)
            {
                Debug.LogWarning("[StudioSplash] promo clip missing — FTUE film skipped.");
                yield break;
            }

            // Logo card hands over to a black screen for the film.
            _loading.text = string.Empty;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Dt;
                float k = Mathf.Clamp01(t / 0.3f);
                _content.alpha = 1f - k;
                _plateImg.color = Color.Lerp(Plate, Color.black, k);
                yield return null;
            }
            _content.alpha = 0f;
            _plateImg.color = Color.black;

            // Letterboxed screen sized to the clip's aspect inside the 1080x1920 frame.
            var rtTex = new RenderTexture((int)clip.width, (int)clip.height, 0);
            float fit = Mathf.Min(1080f / clip.width, 1920f / clip.height);
            var screenRt = MakeRect("PromoScreen", transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(clip.width * fit, clip.height * fit), Vector2.zero);
            var raw = screenRt.gameObject.AddComponent<RawImage>();
            raw.texture = rtTex;
            raw.raycastTarget = false;

            var audio = gameObject.AddComponent<AudioSource>();
            var vp = gameObject.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.clip            = clip;
            vp.renderMode      = VideoRenderMode.RenderTexture;
            vp.targetTexture   = rtTex;
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.SetTargetAudioSource(0, audio);
            vp.isLooping       = false;

            vp.Prepare();
            float prep = 0f;
            while (!vp.isPrepared && prep < 6f) { prep += Dt; yield return null; }

            if (vp.isPrepared)
            {
                vp.Play();
                _skipRequested = false;
                float played = 0f;
                while (played < 0.5f) { played += Dt; yield return null; } // isPlaying settles
                while (vp.isPlaying)
                {
                    played += Dt;
                    if (_skipRequested && played >= 1f) break; // tap to skip
                    yield return null;
                }
                vp.Stop();
            }
            else
            {
                Debug.LogWarning("[StudioSplash] promo clip failed to prepare — film skipped.");
            }

            PlayerPrefs.SetInt(PromoSeenKey, 1);
            PlayerPrefs.Save();

            raw.enabled = false;
            rtTex.Release();
            Destroy(rtTex);
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

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
        MusicManagerMB _music; // non-null only while the film has the soundtrack muted

        /// <summary>True while the boot overlay (logo card and/or promo film)
        /// still covers the game. FTUE choreography holds on this so the intro
        /// dialogue can't boot underneath.</summary>
        public static bool Showing { get; private set; }

        const string PromoSeenKey       = "aq.ftue.promo.seen";
        const string PromoResource      = "App/Video/ally_promo_15s";
        const string PromoAudioResource = "App/Video/ally_promo_15s_audio";

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

        // Skip-tap via TapRouter (2026-08-18): the splash is the topmost surface
        // in the game, so it registers at int.MaxValue and, while showing, claims
        // every tap — nothing underneath (board, dialogue, HUD) can consume the
        // skip tap or be poked through the card.
        AQ.App.UI.TapRouter.Region _tapRegion;

        void OnEnable()
        {
            _tapRegion = AQ.App.UI.TapRouter.Register("studio-splash-skip", int.MaxValue,
                contains: _ => true,
                onTap:    _ => _skipRequested = true,
                enabled:  () => this != null && isActiveAndEnabled);
        }

        void OnDisable()
        {
            AQ.App.UI.TapRouter.Unregister(_tapRegion);
            _tapRegion = null;
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
                // A finger already down when the pump/region came up (icon-tap
                // held through launch, palm-edge touch) never produces a Began
                // event for the router — the old any-touch poll accepted it, so
                // keep that as a fallback alongside the region's proper claim.
                if (Input.touchCount > 0 || Input.GetMouseButton(0)) _skipRequested = true;
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

        void OnDestroy()
        {
            Showing = false;
            _music?.RestoreVolume(); // splash dying mid-film must not leave the BGM muted
        }

        IEnumerator PromoStage()
        {
            if (PlayerPrefs.GetInt(PromoSeenKey, 0) != 0) yield break;

            var clip = Resources.Load<VideoClip>(PromoResource);
            if (clip == null)
            {
                Debug.LogWarning("[StudioSplash] promo clip missing — FTUE film skipped.");
                yield break;
            }
            // 1080x1920 = current H.264 encode; 2160x3840 means Unity is still
            // serving the stale import of the original HEVC master.
            Debug.Log($"[StudioSplash] promo clip {clip.width}x{clip.height} @{clip.frameRate:0}fps, {clip.length:0.0}s");

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
            float fit = Mathf.Min(1080f / clip.width, 1920f / clip.height);
            var screenRt = MakeRect("PromoScreen", transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(clip.width * fit, clip.height * fit), Vector2.zero);
            var raw = screenRt.gameObject.AddComponent<RawImage>();
            raw.color = Color.black; // until the first decoded frame arrives
            raw.raycastTarget = false;

            // The film ships as a SILENT video + separate mp3, played together.
            // Unity's VideoPlayer audio integration (AudioSource and Direct
            // modes both) overflowed AudioSampleProvider on the dev box, and
            // since embedded audio is the video's master clock the film
            // "finished" in ~2s of garble. With no audio track the video runs
            // on game time and AudioSource playback is the same battle-tested
            // path as the rest of the game's sound.
            // APIOnly: sample the player's internal decoded texture directly on
            // the RawImage. The RenderTexture target path never presented a frame
            // on the dev box (black film, zero errors) across every encode.
            var vp = gameObject.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.clip            = clip;
            vp.renderMode      = VideoRenderMode.APIOnly;
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.isLooping       = false;
            vp.skipOnDrop      = false; // present every frame; nothing is clock-synced to us
            vp.errorReceived  += (_, msg) => Debug.LogWarning($"[StudioSplash] promo playback error: {msg}");

            var audioClip = Resources.Load<AudioClip>(PromoAudioResource);
            var audio = gameObject.AddComponent<AudioSource>();
            audio.playOnAwake  = false;
            audio.spatialBlend = 0f;
            audio.volume       = Audio.AudioSettingsService.MusicVolume;

            vp.Prepare();
            float prep = 0f;
            while (!vp.isPrepared && prep < 6f) { prep += Dt; yield return null; }

            if (vp.isPrepared)
            {
                // The film owns the soundtrack (Stephen-ruled 2026-08-14: BGM
                // was playing under the intro film). Duck-to-zero via the same
                // mechanism dialogue uses; restored after Stop below.
                _music = FindAnyObjectByType<MusicManagerMB>();
                _music?.SetDuckedVolume(0f);
                vp.Play();
                if (audioClip != null) { audio.clip = audioClip; audio.Play(); }
                else Debug.LogWarning("[StudioSplash] promo audio clip missing — film plays silent.");
                _skipRequested = false;
                float played = 0f;
                bool diagLogged = false;
                while (played < 0.5f) { played += Dt; yield return null; } // isPlaying settles
                while (vp.isPlaying)
                {
                    played += Dt;
                    // APIOnly: the decoded texture appears after the first frame;
                    // hand it to the RawImage as soon as it exists.
                    if (raw.texture == null && vp.texture != null)
                    {
                        raw.texture = vp.texture;
                        raw.color   = Color.white;
                    }
                    if (!diagLogged && played >= 2f)
                    {
                        diagLogged = true;
                        Debug.Log($"[StudioSplash] promo diag @2s: frame={vp.frame}/{vp.frameCount} time={vp.time:0.00} " +
                                  $"vpTex={(vp.texture != null ? vp.texture.width + "x" + vp.texture.height : "null")}");
                    }
                    if (_skipRequested && played >= 1f) break; // tap to skip
                    yield return null;
                }
                Debug.Log($"[StudioSplash] promo ended: frame={vp.frame}/{vp.frameCount} played={played:0.0}s");
                vp.Stop();
                audio.Stop();
                _music?.RestoreVolume();
                _music = null;

                // Seen only counts when playback actually ran — a failed prepare
                // must not burn the one-shot FTUE film (it burned Stephen's on
                // 2026-08-04 when the editor couldn't decode the old HEVC master).
                PlayerPrefs.SetInt(PromoSeenKey, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("[StudioSplash] promo clip failed to prepare — film skipped, seen-flag NOT set (will retry next boot).");
            }

            raw.enabled = false;
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

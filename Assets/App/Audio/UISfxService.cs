using UnityEngine;

namespace AQ.App.Audio
{
    /// <summary>
    /// Lightweight one-shot SFX player for non-board UI sounds.
    /// Auto-created at runtime — no scene wiring required.
    /// Config loaded from Resources/App/UISfx.
    /// </summary>
    public static class UISfxService
    {
        private static AudioSource _source;
        private static UISfxSO    _config;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            _config = Resources.Load<UISfxSO>("App/UISfx");

            var go = new GameObject("[UISfxService]");
            Object.DontDestroyOnLoad(go);
            _source = go.AddComponent<AudioSource>();
            _source.playOnAwake  = false;
            _source.loop         = false;
            _source.spatialBlend = 0f;
        }

        public static void PlayLeadFulfilled() => Play(_config?.leadFulfilled);
        public static void PlayOverflowDrop()  => Play(_config?.overflowDrop);

        private static void Play(AudioClip clip)
        {
            if (_source == null || clip == null) return;
            _source.PlayOneShot(clip, _config?.volume ?? 0.8f);
        }
    }
}

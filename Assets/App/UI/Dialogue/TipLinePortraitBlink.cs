using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Dialogue
{
    /// <summary>
    /// The Tip Line's slow red blink (Stephen-ruled 2026-08-12): while the
    /// dialogue portrait shows the machine's lit frame, briefly swap to the
    /// unlit frame every couple of seconds — a message light pulsing. Lives on
    /// the portrait Image and only ever touches char_tipline_* sprites, so
    /// every other character (including Ally's animator rig) is untouched.
    /// Generalises to any future two-frame character: add a case to UnlitFor.
    /// </summary>
    public sealed class TipLinePortraitBlink : MonoBehaviour
    {
        private const float Period = 2.6f;  // full cycle
        private const float Unlit  = 0.28f; // dark window at cycle start

        private Image  _img;
        private Sprite _unlit;
        private Sprite _lit;
        private float  _t;

        private void Awake()
        {
            _img   = GetComponent<Image>();
            _unlit = Resources.Load<Sprite>("App/Characters/char_tipline_neutral_f01");
        }

        private void Update()
        {
            if (_img == null || _unlit == null) return;

            var s = _img.sprite;
            if (s == null) { _lit = null; return; }
            if (s != _unlit)
            {
                // The controller (re)assigned a sprite: track it if it is the
                // machine, stand down for anyone else.
                if (!s.name.StartsWith("char_tipline")) { _lit = null; return; }
                if (_lit != s) { _lit = s; _t = Unlit; } // start lit, not mid-blink
            }
            if (_lit == null) return;

            _t += Time.unscaledDeltaTime;
            var want = (_t % Period) < Unlit ? _unlit : _lit;
            if (_img.sprite != want) _img.sprite = want;
        }
    }

    /// <summary>Installs the blink onto the dialogue portrait once the panel
    /// exists (it sleeps between dialogues, so scan until found).</summary>
    public static class TipLineBlinkBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var host = new GameObject("[TipLineBlink]");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<Installer>();
        }

        private sealed class Installer : MonoBehaviour
        {
            private float _nextScan;

            private void Update()
            {
                if (Time.unscaledTime < _nextScan) return;
                _nextScan = Time.unscaledTime + 1f;

                var dc = FindAnyObjectByType<DialogueController>(FindObjectsInactive.Include);
                if (dc == null || dc.portraitImage == null) return;
                if (dc.portraitImage.GetComponent<TipLinePortraitBlink>() == null)
                    dc.portraitImage.gameObject.AddComponent<TipLinePortraitBlink>();
                enabled = false; // installed — stop scanning
            }
        }
    }
}

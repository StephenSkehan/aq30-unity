using System.Collections;
using UnityEngine;

namespace AQ.App.UI
{
    /// Fades a CanvasGroup 0→maxAlpha→0 over 'duration' seconds using unscaled time.
    /// Use with an Image whose Color alpha is 1.0 so CanvasGroup controls the opacity.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FlashOnceMB : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0.01f)] [SerializeField] private float duration = 0.30f;
        [Tooltip("If true, plays once on Start (useful for testing).")]
        [SerializeField] private bool triggerOnStart = false;

        [Header("Look")]
        [Range(0f, 1f)] [SerializeField] private float maxAlpha = 1f;
        [Tooltip("Optional shape of the flash (0→1→0). Leave empty to use a sine arch.")]
        [SerializeField] private AnimationCurve customCurve;

        [Header("Order")]
        [Tooltip("Move to top of its siblings while flashing so it renders above other UI.")]
        [SerializeField] private bool setAsLastSiblingWhileFlashing = true;

        private CanvasGroup _cg;
        private Coroutine _running;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>();
            _cg.alpha = 0f;
            _cg.interactable = false;
            _cg.blocksRaycasts = false;

            // If no curve set in Inspector, we’ll just use a sine arch.
            // (0→1→0 look; you can assign a 4-key plateau curve in the Inspector for extra punch.)
        }

        private void Start()
        {
            if (triggerOnStart) Trigger();
        }

        public void Trigger() => TriggerFor(duration);

        public void TriggerFor(float seconds)
        {
            duration = Mathf.Max(0.01f, seconds);
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(CoFlash());
        }

        private IEnumerator CoFlash()
        {
            if (setAsLastSiblingWhileFlashing) transform.SetAsLastSibling();

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / duration);
                float shape = EvaluateShape(n);     // 0..1
                _cg.alpha = Mathf.Clamp01(shape * maxAlpha);
                yield return null;
            }

            _cg.alpha = 0f;
            _running = null;
        }

        private float EvaluateShape(float n)
        {
            if (customCurve != null && customCurve.length >= 2)
                return Mathf.Clamp01(customCurve.Evaluate(n));
            // Default: quick, readable 0→1→0 arch
            return Mathf.Sin(n * Mathf.PI);
        }
    }
}

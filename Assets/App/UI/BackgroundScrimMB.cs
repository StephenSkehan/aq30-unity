using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI
{
    /// <summary>
    /// Tunable dark scrim that sits between the scene background and the board so
    /// the board reads as the focal plane. The Rivermouth art is intentionally
    /// dark — tune this scrim rather than lightening the art. Opacity is a
    /// serialized [0..1] value; [ExecuteAlways] so it updates live in the editor.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public sealed class BackgroundScrimMB : MonoBehaviour
    {
        [Range(0f, 1f)]
        [Tooltip("0 = fully transparent (art at full brightness), 1 = solid scrim colour.")]
        public float opacity = 0.4f;

        [Tooltip("Scrim colour (usually near-black). Alpha is driven by Opacity.")]
        public Color tint = Color.black;

        private Image _img;

        private void Awake()     => Apply();
        private void OnEnable()  => Apply();
        private void OnValidate()=> Apply();

        public void SetOpacity(float value)
        {
            opacity = Mathf.Clamp01(value);
            Apply();
        }

        private void Apply()
        {
            if (!_img) _img = GetComponent<Image>();
            if (!_img) return;
            var c = tint;
            c.a = opacity;
            _img.color = c;
            _img.raycastTarget = false;
        }
    }
}

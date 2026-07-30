using System.Collections;
using AQ.App.Generators;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board.FX
{
    /// <summary>
    /// Star-glint twinkle over generator tiles (full-tile glow wash retired,
    /// Stephen-ruled 2026-07-24 — the tap signal is the fading energy badge).
    /// Attach via MergeBoardController.AttachGeneratorAnimator() — never add manually.
    /// Colors/emission stay designer-tunable via GeneratorTierParticleConfig.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GeneratorTileAnimator : MonoBehaviour
    {
        private GeneratorTierParticleConfig _config;
        private RectTransform               _tileRect;
        private bool                        _active;

        private const float GlintIn    = 0.22f;
        private const float GlintHold  = 0.10f;
        private const float GlintOut   = 0.40f;
        private const float GlintAlpha = 0.9f;

        // Four-point star sprite, generated once. A default Image quad is a
        // hard-edged square (the original "pixelated noise"), and plain soft
        // dots read as bubbles — this is the classic lens-glint shape.
        private static Sprite _glintSprite;

        private static Sprite GlintSprite
        {
            get
            {
                if (_glintSprite != null) return _glintSprite;

                const int size = 96;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode  = TextureWrapMode.Clamp
                };
                var px   = new Color32[size * size];
                float half = size / 2f;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f - half) / half; // -1..1
                    float v = (y + 0.5f - half) / half;

                    // spikes taper as they leave the centre
                    float wV = 0.16f * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(v)), 1.6f) + 0.012f;
                    float sV = Mathf.Max(0f, 1f - Mathf.Abs(u) / wV) * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(v)), 1.2f);
                    float wH = 0.16f * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(u)), 1.6f) + 0.012f;
                    float sH = Mathf.Max(0f, 1f - Mathf.Abs(v) / wH) * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(u)), 1.2f);

                    float r    = Mathf.Sqrt(u * u + v * v);
                    float core = Mathf.Pow(Mathf.Max(0f, 1f - r / 0.28f), 2f);

                    float a = Mathf.Clamp01(Mathf.Max(Mathf.Max(sV, sH), core));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
                tex.SetPixels32(px);
                tex.Apply(false, true);

                _glintSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
                _glintSprite.name = "GGlintStar";
                _glintSprite.hideFlags = HideFlags.HideAndDontSave;
                return _glintSprite;
            }
        }

        // ---- Public API ----

        public void Init(GeneratorTierParticleConfig config, RectTransform tileRect)
        {
            _config   = config;
            _tileRect = tileRect;

            if (!_active)
            {
                _active = true;
                StartCoroutine(GlintEmitter());
            }
        }

        public void Teardown()
        {
            _active = false;

            if (_tileRect != null)
            {
                for (int i = _tileRect.childCount - 1; i >= 0; i--)
                {
                    var child = _tileRect.GetChild(i);
                    if (child.name == "GGlint" || child.name == "GMote" || child.name == "GSpark" || child.name == "GGlow")
                        Destroy(child.gameObject);
                }
            }

            Destroy(this);
        }

        private void OnDestroy() => _active = false;

        // ---- Glint emitter ----

        private IEnumerator GlintEmitter()
        {
            while (_active)
            {
                float interval = Mathf.Max(0.5f, 1f / Mathf.Max(0.1f, _config.emissionRate));
                yield return new WaitForSeconds(interval * Random.Range(0.9f, 1.6f));
                if (_active && _tileRect) EmitGlint();
            }
        }

        private void EmitGlint()
        {
            var go = new GameObject("GGlint");
            go.transform.SetParent(_tileRect, false);

            var rt       = go.AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * Random.Range(20f, 32f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));

            // bias toward the upper half — glints read as light catching an edge
            float hw = _tileRect.rect.width  * 0.36f;
            float hh = _tileRect.rect.height * 0.34f;
            rt.anchoredPosition = new Vector2(Random.Range(-hw, hw), Random.Range(-hh * 0.4f, hh));

            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.sprite = GlintSprite;
            img.raycastTarget = false;
            img.color = Color.Lerp(_config.startColor, Color.white, 0.65f);

            StartCoroutine(Twinkle(rt, img));
        }

        private IEnumerator Twinkle(RectTransform rt, Image img)
        {
            var baseColor = img.color;
            float life    = GlintIn + GlintHold + GlintOut;
            float elapsed = 0f;

            while (elapsed < life && img)
            {
                elapsed += Time.deltaTime;

                float s;
                if      (elapsed < GlintIn)            s = elapsed / GlintIn;
                else if (elapsed < GlintIn + GlintHold) s = 1f;
                else                                    s = 1f - (elapsed - GlintIn - GlintHold) / GlintOut;
                s = Mathf.Clamp01(s);
                s = s * s * (3f - 2f * s); // smooth both ends of the twinkle

                rt.localScale = Vector3.one * s;
                rt.Rotate(0f, 0f, 24f * Time.deltaTime);
                var c = baseColor; c.a = s * GlintAlpha;
                img.color = c;

                yield return null;
            }
            if (img) Destroy(img.gameObject);
        }
    }
}

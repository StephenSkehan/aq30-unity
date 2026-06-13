using System.Collections;
using AQ.App.Generators;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board.FX
{
    /// <summary>
    /// UI-native glow + spark animation for generator tiles.
    /// Attach via MergeBoardController.AttachGeneratorAnimator() — never add manually.
    /// Driven entirely by GeneratorTierParticleConfig so all values are designer-tunable in GeneratorTypeSO.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GeneratorTileAnimator : MonoBehaviour
    {
        private GeneratorTierParticleConfig _config;
        private RectTransform               _tileRect;
        private Image                       _glow;
        private bool                        _active;

        private const float GlowMaxAlpha  = 0.30f;
        private const float SparkSizePx   = 10f;
        private const float SparkLifetime = 0.45f;

        // ---- Public API ----

        public void Init(GeneratorTierParticleConfig config, RectTransform tileRect)
        {
            _config   = config;
            _tileRect = tileRect;

            EnsureGlow();

            if (!_active)
            {
                _active = true;
                StartCoroutine(GlowPulse());
                StartCoroutine(SparkEmitter());
            }
        }

        public void Teardown()
        {
            _active = false;

            if (_glow) Destroy(_glow.gameObject);
            _glow = null;

            // Destroy any in-flight sparks left on the tile transform
            if (_tileRect != null)
            {
                for (int i = _tileRect.childCount - 1; i >= 0; i--)
                {
                    var child = _tileRect.GetChild(i);
                    if (child.name == "GSpark") Destroy(child.gameObject);
                }
            }

            Destroy(this);
        }

        private void OnDestroy() => _active = false;

        // ---- Glow overlay ----

        private void EnsureGlow()
        {
            if (_glow) return;

            var go = new GameObject("GGlow");
            go.transform.SetParent(_tileRect, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<CanvasRenderer>();
            _glow = go.AddComponent<Image>();
            _glow.raycastTarget = false;
            var c = _config.startColor; c.a = 0f;
            _glow.color = c;
        }

        private IEnumerator GlowPulse()
        {
            float t     = 0f;
            float speed = 0.5f; // 1/speed = 2-second cycle

            while (_active && _glow)
            {
                t += Time.deltaTime * speed;
                float alpha   = (Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f) * GlowMaxAlpha;
                float lerp    = Mathf.Sin(t * Mathf.PI) * 0.5f + 0.5f;
                var   col     = Color.Lerp(_config.startColor, _config.endColor, lerp);
                col.a         = alpha;
                _glow.color   = col;
                yield return null;
            }
        }

        // ---- Spark emitter ----

        private IEnumerator SparkEmitter()
        {
            while (_active)
            {
                float interval = Mathf.Max(0.05f, 1f / Mathf.Max(0.1f, _config.emissionRate));
                yield return new WaitForSeconds(interval);
                if (_active && _tileRect) EmitSpark();
            }
        }

        private void EmitSpark()
        {
            var go = new GameObject("GSpark");
            go.transform.SetParent(_tileRect, false);

            var rt          = go.AddComponent<RectTransform>();
            rt.sizeDelta    = Vector2.one * SparkSizePx;
            rt.anchorMin    = rt.anchorMax = new Vector2(0.5f, 0.5f);

            float hw = _tileRect.rect.width  * 0.38f;
            float hh = _tileRect.rect.height * 0.38f;
            rt.anchoredPosition = new Vector2(Random.Range(-hw, hw), Random.Range(-hh, hh));

            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = _config.startColor;

            StartCoroutine(FadeSpark(img));
        }

        private IEnumerator FadeSpark(Image img)
        {
            float elapsed = 0f;
            while (elapsed < SparkLifetime && img)
            {
                elapsed    += Time.deltaTime;
                img.color   = Color.Lerp(_config.startColor, _config.endColor, elapsed / SparkLifetime);
                yield return null;
            }
            if (img) Destroy(img.gameObject);
        }
    }
}

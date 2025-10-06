using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Simple screen-space toast service with FIFO queue and duplicate coalescing.
    /// Auto-creates itself on first use; no scene setup required. No built-in resource lookups (silent).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ToastService : MonoBehaviour
    {
        private const string ServiceName = "__AQ_ToastService";
        private const float DuplicateWindowSeconds = 1.0f;

        private static ToastService _instance;

        private readonly Queue<(string id, string text, float duration)> _queue = new();
        private readonly Dictionary<string, float> _lastShownById = new();
        private bool _isShowing;

        private Canvas _canvas;
        private CanvasGroup _group;
        private RectTransform _panel;
        private Image _panelImage;
        private TextMeshProUGUI _label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (_instance == null) CreateInstance();
        }

        /// <summary>Show a toast. Duplicates with the same id inside a short window are coalesced.</summary>
        public static void Show(string id, string text, float duration = 1.75f)
        {
            if (_instance == null) CreateInstance();

            var now = Time.unscaledTime;
            if (!string.IsNullOrEmpty(id) && _instance._lastShownById.TryGetValue(id, out var last))
            {
                if (now - last < DuplicateWindowSeconds) return;
            }
            if (!string.IsNullOrEmpty(id))
                _instance._lastShownById[id] = now;

            _instance._queue.Enqueue((id ?? string.Empty, text ?? string.Empty, Mathf.Max(0.25f, duration)));
            if (!_instance._isShowing)
                _instance.StartCoroutine(_instance.ProcessQueue());
        }

        private static void CreateInstance()
        {
            var go = new GameObject(ServiceName);
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<ToastService>();
            _instance.BuildUI();
        }

        private void BuildUI()
        {
            // Canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // Panel (top-center)
            var panelGO = new GameObject("ToastPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panelGO.transform.SetParent(transform, false);
            _panel = panelGO.GetComponent<RectTransform>();
            _panelImage = panelGO.GetComponent<Image>();
            _group = panelGO.GetComponent<CanvasGroup>();

            _panelImage.raycastTarget = false;
            _panelImage.color = new Color(0f, 0f, 0f, 0.6f);
            _panelImage.sprite = CreateFallbackSprite(); // no Resources lookups
            _panelImage.type = Image.Type.Simple;

            _panel.anchorMin = new Vector2(0.5f, 1f);
            _panel.anchorMax = new Vector2(0.5f, 1f);
            _panel.pivot = new Vector2(0.5f, 1f);
            _panel.anchoredPosition = new Vector2(0f, -80f);
            _panel.sizeDelta = new Vector2(800f, 64f);

            // Label (TMP)
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(panelGO.transform, false);
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(1f, 1f);
            lrt.offsetMin = new Vector2(24f, 12f);
            lrt.offsetMax = new Vector2(-24f, -12f);

            _label = labelGO.AddComponent<TextMeshProUGUI>();
            _label.raycastTarget = false;
            _label.textWrappingMode = TextWrappingModes.Normal; // no obsolete warning
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = 28f;
            _label.text = "";

            // Hidden by default
            _group.alpha = 0f;
            _panel.gameObject.SetActive(false);
        }

        private IEnumerator ProcessQueue()
        {
            _isShowing = true;

            while (_queue.Count > 0)
            {
                var (_, text, duration) = _queue.Dequeue();

                _label.text = text;
                _panel.gameObject.SetActive(true);

                yield return FadeTo(1f, 0.12f);

                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                yield return FadeTo(0f, 0.15f);

                _panel.gameObject.SetActive(false);
            }

            _isShowing = false;
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            float start = _group.alpha;
            float t = 0f;
            seconds = Mathf.Max(0.01f, seconds);

            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / seconds);
                _group.alpha = Mathf.Lerp(start, target, p);
                yield return null;
            }

            _group.alpha = target;
        }

        // ---------- Sprite helper (silent) ----------

        private static Sprite CreateFallbackSprite()
        {
            // Simple 1x1 white texture; Image.color provides tint/alpha.
            const int size = 1;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "__AQ_ToastFallback";
            tex.SetPixel(0, 0, Color.white);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

// Assets/Scripts/UI/Board/FX/BoardFxPlayer.cs
using System.Collections;
using AQ.App.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Plays spawn/merge/swap/move feedback safely over your existing board.
    /// Creates a non-blocking overlay under the board for slide/sparkle.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MergeBoardController))]
    public sealed class BoardFxPlayer : MonoBehaviour
    {
        [SerializeField] BoardFxConfigSO config;
        [SerializeField] RectTransform overlay;     // created at runtime if missing
        [Header("Debug")]
        [SerializeField] bool debugLogs = false;    // set true to see Play* logs in Console

        MergeBoardController ctrl;
        Canvas parentCanvas;
        Camera uiCam;

        const string OverlayName = "__AQ_FXOverlay";

        void Awake()
        {
            ctrl = GetComponent<MergeBoardController>();
            if (!ctrl) { enabled = false; return; }

            parentCanvas = ctrl.boardRoot ? ctrl.boardRoot.GetComponentInParent<Canvas>() : null;
            uiCam = parentCanvas && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? parentCanvas.worldCamera
                : null;

            EnsureOverlay();
        }

        public void SetConfig(BoardFxConfigSO cfg) => config = cfg;

        void EnsureOverlay()
        {
            if (overlay) return;

            var parent = ctrl.boardRoot ? ctrl.boardRoot : (RectTransform)transform;
            var t = parent.Find(OverlayName) as RectTransform;
            if (!t)
            {
                var go = new GameObject(OverlayName, typeof(RectTransform), typeof(CanvasGroup));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
                t.anchorMin = Vector2.zero;
                t.anchorMax = Vector2.one;
                t.offsetMin = Vector2.zero;
                t.offsetMax = Vector2.zero;

                var cg = go.GetComponent<CanvasGroup>();
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
            overlay = t;
            overlay.SetAsLastSibling(); // above board visuals
        }

        // --- Public API used by the observer ---

        public void PlaySpawn(BoardTileView tile)
        {
            if (debugLogs) Debug.Log("[FX] Spawn");
            if (!tile || !tile.itemImage) return;
            UISfxService.PlayBoardSpawn();

            var rt = tile.itemImage.rectTransform;
            StartCoroutine(CoPop(rt,
                startScale: config ? config.spawnStartScale : 0.85f,
                peakScale : config ? config.popPeakScale     : 1.15f,
                duration  : config ? config.spawnPopDuration : 0.12f));
            SparkleBurst(rt, 6, 46f, 0.35f);
        }

        public void PlayMerge(BoardTileView from, BoardTileView into, Sprite _unused)
        {
            if (debugLogs) Debug.Log("[FX] Merge");
            UISfxService.PlayBoardMerge();

            // Pop the destination tile
            if (into && into.itemImage)
            {
                StartCoroutine(CoPop(into.itemImage.rectTransform,
                    startScale: 1f,
                    peakScale : config ? config.popPeakScale     : 1.15f,
                    duration  : config ? config.mergePopDuration : 0.12f));
            }

            // Sparkle at destination: configured prefab if one exists, otherwise
            // the built-in procedural burst (bigger than the spawn burst).
            if (config && config.sparklePrefab && into && into.itemImage)
            {
                var pos = WorldToOverlayPoint(into.itemImage.rectTransform);
                if (config.sparkleIsUI)
                {
                    var fx = Instantiate(config.sparklePrefab, overlay);
                    var fxRt = fx.transform as RectTransform;
                    fxRt.anchoredPosition = pos;
                    fxRt.localScale = Vector3.one;
                    fx.Play();
                    Destroy(fx.gameObject, config.sparkleLifetime);
                }
                else
                {
                    var fx = Instantiate(config.sparklePrefab, overlay.transform);
                    fx.transform.position = into.itemImage.rectTransform.position;
                    fx.Play();
                    Destroy(fx.gameObject, config.sparkleLifetime);
                }
            }
            else if (into && into.itemImage)
            {
                SparkleBurst(into.itemImage.rectTransform, 8, 68f, 0.45f);
            }

            // Slide of the consumed piece into the destination
            if (from && from.itemImage && into && into.itemImage)
            {
                var a = CreateOverlayIcon(from.itemImage.sprite, from.itemImage.rectTransform);
                var start = WorldToOverlayPoint(from.itemImage.rectTransform);
                var end   = WorldToOverlayPoint(into.itemImage.rectTransform);
                a.rectTransform.anchoredPosition = start;
                StartCoroutine(CoSlideAndDispose(a, start, end, config ? config.mergePopDuration : 0.12f));
            }
        }

        public void PlaySwap(BoardTileView a, BoardTileView b)
        {
            if (debugLogs) Debug.Log("[FX] Swap");
            if (!a || !b || !a.itemImage || !b.itemImage) return;
            UISfxService.PlayBoardSwap();

            // Hide real icons during slide (prevents double images)
            var aImg = a.itemImage; var bImg = b.itemImage;
            var aWas = aImg.enabled; var bWas = bImg.enabled;
            aImg.enabled = false; bImg.enabled = false;

            // Create overlay icons
            var overlayA = CreateOverlayIcon(aImg.sprite, aImg.rectTransform);
            var overlayB = CreateOverlayIcon(bImg.sprite, bImg.rectTransform);

            var aStart = WorldToOverlayPoint(aImg.rectTransform);
            var bStart = WorldToOverlayPoint(bImg.rectTransform);
            overlayA.rectTransform.anchoredPosition = aStart;
            overlayB.rectTransform.anchoredPosition = bStart;

            var dur = config ? config.swapSlideDuration : 0.12f;
            StartCoroutine(CoSlideAndDispose(overlayA, aStart, bStart, dur, () => aImg.enabled = aWas));
            StartCoroutine(CoSlideAndDispose(overlayB, bStart, aStart, dur, () => bImg.enabled = bWas));
        }

        // NEW: subtle feedback when moving into empty cell
        public void PlayMove(BoardTileView into)
        {
            if (debugLogs) Debug.Log("[FX] Move");
            if (!into || !into.itemImage) return;
            StartCoroutine(CoPop(into.itemImage.rectTransform,
                startScale: 1.0f,
                peakScale : 1.08f,
                duration  : 0.08f));
        }

        // --- Helpers ---

        static readonly Color kSparkleWarm = new Color(0.99f, 0.83f, 0.45f, 1f); // amber-white

        /// <summary>
        /// Procedural UI sparkle: small rotated-square motes radiating from the
        /// tile centre, shrinking and fading. No prefab or particle system.
        /// </summary>
        void SparkleBurst(RectTransform target, int count, float radius, float duration)
        {
            if (!target || !overlay) return;
            var centre = WorldToOverlayPoint(target);
            for (int i = 0; i < count; i++)
            {
                float ang = (360f / count) * i + Random.Range(-14f, 14f);
                var dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                float dist = radius * Random.Range(0.7f, 1.15f);
                float size = Random.Range(7f, 13f);

                var go = new GameObject("FX_Spark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(overlay, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = centre;
                rt.localRotation = Quaternion.Euler(0f, 0f, 45f);

                var img = go.GetComponent<Image>();
                img.color = i % 2 == 0 ? Color.white : kSparkleWarm;
                img.raycastTarget = false;

                StartCoroutine(CoSpark(rt, img, centre, centre + dir * dist, duration * Random.Range(0.85f, 1.15f)));
            }
        }

        IEnumerator CoSpark(RectTransform rt, Image img, Vector2 from, Vector2 to, float duration)
        {
            var t0 = Time.unscaledTime;
            var startSize = rt.sizeDelta;
            while (Time.unscaledTime - t0 < duration)
            {
                if (!rt) yield break;
                var u = Mathf.Clamp01((Time.unscaledTime - t0) / duration);
                rt.anchoredPosition = Vector2.Lerp(from, to, Smooth(u));
                rt.sizeDelta = startSize * (1f - u * 0.8f);
                var c = img.color; c.a = 1f - u * u; img.color = c;
                yield return null;
            }
            if (rt) Destroy(rt.gameObject);
        }

        Vector2 WorldToOverlayPoint(RectTransform worldTarget)
        {
            var sp = RectTransformUtility.WorldToScreenPoint(uiCam, worldTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, sp, uiCam, out var local);
            return local;
        }

        Image CreateOverlayIcon(Sprite sprite, RectTransform likeSize)
        {
            var go = new GameObject("FX_Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(overlay, false);
            rt.sizeDelta = likeSize.rect.size;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        IEnumerator CoPop(RectTransform target, float startScale, float peakScale, float duration)
        {
            if (!target) yield break;
            var t0 = Time.unscaledTime;
            var mid = duration * 0.5f;
            var orig = target.localScale;
            target.localScale = Vector3.one * startScale;

            // Up
            while (Time.unscaledTime - t0 < mid)
            {
                var u = Mathf.Clamp01((Time.unscaledTime - t0) / mid);
                var s = Mathf.Lerp(startScale, peakScale, Smooth(u));
                target.localScale = Vector3.one * s;
                yield return null;
            }

            // Down
            var t1 = Time.unscaledTime;
            while (Time.unscaledTime - t1 < mid)
            {
                var u = Mathf.Clamp01((Time.unscaledTime - t1) / mid);
                var s = Mathf.Lerp(peakScale, 1f, Smooth(u));
                target.localScale = Vector3.one * s;
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        IEnumerator CoSlideAndDispose(Image icon, Vector2 from, Vector2 to, float duration, System.Action onDone = null)
        {
            if (!icon) yield break;
            var t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < duration)
            {
                var u = Mathf.Clamp01((Time.unscaledTime - t0) / duration);
                icon.rectTransform.anchoredPosition = Vector2.Lerp(from, to, Smooth(u));
                yield return null;
            }
            icon.rectTransform.anchoredPosition = to;
            if (icon) Destroy(icon.gameObject);
            onDone?.Invoke();
        }

        static float Smooth(float x) => x * x * (3f - 2f * x); // smoothstep
    }
}

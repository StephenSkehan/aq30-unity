using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI
{
    /// <summary>
    /// Reward flight particles: small currency chips arc from the leads bar
    /// to their HUD counter when a lead pays out. Self-hosting (own overlay
    /// canvas + coroutine runner), no scene wiring required.
    /// </summary>
    public static class FlightFX
    {
        const float Duration = 0.55f;
        const float Stagger  = 0.07f;
        const float ChipSize = 56f;

        static FlightRunner _runner;

        /// <summary>kind: "soft" | "energy" | "premium".</summary>
        public static void FlyReward(string kind, int amount)
        {
            if (amount <= 0) return;

            Sprite sprite; string targetName;
            switch (kind)
            {
                case "soft":    sprite = Resources.Load<Sprite>("App/UI/Icons/flight_cash");        targetName = "Txt_Soft_Currency"; break;
                case "energy":  sprite = Resources.Load<Sprite>("App/UI/MergeBoard/energy_badge");  targetName = "Txt_Value";         break;
                case "premium": sprite = Resources.Load<Sprite>("App/UI/Icons/flight_ingot");       targetName = "Txt_Premium";       break;
                default: return;
            }

            var target = GameObject.Find(targetName);
            if (target == null) return;

            var from = SourceScreenPos();
            var to   = RectTransformUtility.WorldToScreenPoint(null, target.transform.position);
            int count = Mathf.Clamp(amount / 20 + 2, 3, 8);

            Runner().StartCoroutine(FlyBatch(sprite, from, to, count, target.transform));
        }

        static Vector2 SourceScreenPos()
        {
            var bar = GameObject.Find("LeadsBarRuntime");
            if (bar != null)
                return RectTransformUtility.WorldToScreenPoint(null, bar.transform.position);
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.7f);
        }

        static FlightRunner Runner()
        {
            if (_runner != null) return _runner;

            var go = new GameObject("__FlightFX", typeof(Canvas), typeof(CanvasScaler));
            Object.DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9100; // above HUD, below modal popups
            go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _runner = go.AddComponent<FlightRunner>();
            return _runner;
        }

        static IEnumerator FlyBatch(Sprite sprite, Vector2 from, Vector2 to, int count, Transform pulseTarget)
        {
            for (int i = 0; i < count; i++)
            {
                Runner().StartCoroutine(FlyOne(sprite, from + Random.insideUnitCircle * 60f, to, pulseTarget));
                yield return new WaitForSeconds(Stagger);
            }
        }

        static IEnumerator FlyOne(Sprite sprite, Vector2 from, Vector2 to, Transform pulseTarget)
        {
            var go = new GameObject("chip", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(Runner().transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(ChipSize, ChipSize);
            rt.anchoredPosition = from;

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            if (sprite == null) img.color = AQTheme.Amber; // still reads as a reward chip

            // control point bows the path sideways and slightly up
            var mid = (from + to) / 2f
                      + Vector2.Perpendicular((to - from).normalized) * Random.Range(-90f, 90f)
                      + Vector2.up * 60f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Duration;
                float e = Mathf.Pow(Mathf.Clamp01(t), 1.7f); // ease-in: hangs, then darts home
                var a = Vector2.Lerp(from, mid, e);
                var b = Vector2.Lerp(mid, to, e);
                rt.anchoredPosition = Vector2.Lerp(a, b, e);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.55f, e);
                yield return null;
            }

            Object.Destroy(go);
            if (pulseTarget != null)
                Runner().StartCoroutine(Pulse(pulseTarget));
        }

        static IEnumerator Pulse(Transform target)
        {
            target.localScale = Vector3.one * 1.14f;
            float t = 0f;
            while (t < 1f && target != null)
            {
                t += Time.unscaledDeltaTime / 0.16f;
                target.localScale = Vector3.one * Mathf.Lerp(1.14f, 1f, t);
                yield return null;
            }
            if (target != null) target.localScale = Vector3.one;
        }

        private sealed class FlightRunner : MonoBehaviour { }
    }
}

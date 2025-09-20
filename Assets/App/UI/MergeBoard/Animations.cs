using System.Collections;
using UnityEngine;

namespace AQ.App
{
    public static class Animations
    {
        public static IEnumerator Pulse(Transform t, float scale = 1.15f, float dur = 0.15f)
        {
            if (t == null) yield break;
            var start = t.localScale;
            var target = start * scale;
            float t0 = 0f;
            while (t0 < dur)
            {
                t0 += Time.deltaTime;
                float k = Mathf.Clamp01(t0 / dur);
                t.localScale = Vector3.Lerp(start, target, k);
                yield return null;
            }
            t0 = 0f;
            while (t0 < dur)
            {
                t0 += Time.deltaTime;
                float k = Mathf.Clamp01(t0 / dur);
                t.localScale = Vector3.Lerp(target, start, k);
                yield return null;
            }
            t.localScale = start;
        }
    }
}
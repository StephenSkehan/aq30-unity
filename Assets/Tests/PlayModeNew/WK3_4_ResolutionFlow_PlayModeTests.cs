using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using AQ.App.CaseFlow; // ResolutionContinueMB

public class WK3_4_ResolutionFlow_PlayModeTests
{
    [UnityTest]
    public IEnumerator ResolutionOverlay_Hides_OnResolve()
    {
        // ---- Setup a minimal overlay ----
        var canvasGO = new GameObject("TestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var overlayGO = new GameObject("ResolutionPanel", typeof(RectTransform), typeof(CanvasGroup));
        overlayGO.transform.SetParent(canvasGO.transform, false);

        // Optional: add a visual so CanvasGroup has a target
        overlayGO.AddComponent<Image>();

        var resolver = overlayGO.AddComponent<ResolutionContinueMB>();
        yield return null; // let Awake/Start (if any) run

        // ---- Reflective wiring so we don't depend on field access levels/names changing ----
        var cg = overlayGO.GetComponent<CanvasGroup>();

        // helper: assign field by name if it exists and types are compatible
        void TrySetField(object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType.IsInstanceOfType(value))
                f.SetValue(target, value);
        }

        // rootToHide can be GameObject or Transform in some variants; try both
        var rootField = resolver.GetType().GetField("rootToHide", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (rootField != null)
        {
            if (rootField.FieldType == typeof(GameObject)) TrySetField(resolver, "rootToHide", overlayGO);
            else if (rootField.FieldType == typeof(Transform)) TrySetField(resolver, "rootToHide", overlayGO.transform);
        }

        // CanvasGroup + fadeDuration (if present)
        TrySetField(resolver, "group", cg);

        // keep fade short for tests if available (and avoid 0 to skip div-by-zero patterns)
        var fadeField = resolver.GetType().GetField("fadeDuration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fadeField != null && fadeField.FieldType == typeof(float))
            fadeField.SetValue(resolver, 0.05f);

        // Sanity preconditions
        Assert.IsTrue(overlayGO.activeSelf, "Overlay should start active");
        if (cg != null) Assert.GreaterOrEqual(cg.alpha, 0.99f, "CanvasGroup should start visible (alpha≈1)");

        // ---- Act ----
        resolver.OnResolve(); // direct call is fine for PlayMode

        // Wait a bit longer than fadeDuration if present
        yield return new WaitForSeconds(0.1f);
        yield return null;

        // ---- Assert: hidden either by SetActive(false) OR by CanvasGroup alpha & interactivity ----
        bool hiddenByDisable = !overlayGO.activeSelf;
        bool hiddenByCanvasGroup = (cg != null) && cg.alpha <= 0.01f && !cg.interactable && !cg.blocksRaycasts;

        Assert.IsTrue(hiddenByDisable || hiddenByCanvasGroup,
            $"Overlay not hidden. active={overlayGO.activeSelf}, " +
            (cg != null ? $"alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}" : "no CanvasGroup"));

        // ---- Cleanup ----
        Object.Destroy(canvasGO);
        yield return null;
    }
}

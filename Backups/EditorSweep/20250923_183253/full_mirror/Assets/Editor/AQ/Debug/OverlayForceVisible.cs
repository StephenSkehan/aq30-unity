#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class OverlayForceVisible
{
    [MenuItem("AQ/Debug/Force SHOW ResolutionRoot")]
    public static void ForceShow()
    {
        var rr = GameObject.Find("ResolutionRoot");
        if (rr == null) { Debug.LogWarning("[OverlayForce] ResolutionRoot not found."); return; }

        rr.SetActive(true);

        var canvas = rr.GetComponent<Canvas>() ?? rr.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9000;

        var cg = rr.GetComponent<CanvasGroup>() ?? rr.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        var img = rr.GetComponent<Image>() ?? rr.AddComponent<Image>();
        if (img.sprite == null) img.color = new Color(1f, 0f, 1f, 0.5f); // magenta, semi-opaque

        var rt = rr.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // If gating is hiding it, disable the gate while we debug.
        var gate = rr.GetComponent<MonoBehaviour>(); // placeholder to avoid hard type ref
        var gateType = rr.GetComponent("CaseFlowGateMB"); // reflection-friendly
        if (gateType != null)
        {
            var gateMB = (MonoBehaviour)gateType;
            gateMB.enabled = false;
            Debug.Log("[OverlayForce] Disabled CaseFlowGateMB during debug.");
        }

        Selection.activeGameObject = rr;
        Debug.Log("[OverlayForce] ResolutionRoot forced visible at top with sortingOrder=9000.");
    }
}
#endif

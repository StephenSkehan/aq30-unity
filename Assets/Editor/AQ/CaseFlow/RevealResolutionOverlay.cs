#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

static class AQ_RevealResolutionOverlay
{
    [MenuItem("AQ/CaseFlow/Reveal Resolution Overlay (nuclear)")]
    static void Reveal()
    {
        // 1) Find or create the overlay root
        var go = GameObject.Find("ResolutionRoot");
        if (go == null)
        {
            go = new GameObject("ResolutionRoot", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create ResolutionRoot");
        }

        // 2) Reparent to scene root (escape any parent CanvasGroup fading / gates)
        go.transform.SetParent(null, worldPositionStays: false);

        // 3) Ensure Canvas configured to render above everything
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null) canvas = Undo.AddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9000; // huge, above other UIs

        if (go.GetComponent<GraphicRaycaster>() == null)
            Undo.AddComponent<GraphicRaycaster>(go);

        // 4) Fullscreen stretch
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // 5) Add a loud background so it's impossible to miss
        var img = go.GetComponent<Image>();
        if (img == null) img = Undo.AddComponent<Image>(go);
        // bright magenta with some transparency
        img.color = new Color(1f, 0f, 1f, 0.55f);

        // 6) Ensure visible regardless of parent groups
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = Undo.AddComponent<CanvasGroup>(go);
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

        // 7) Add a big label
        var label = (GameObject)GameObject.Find("ResolutionRoot/Title");
        if (label == null)
        {
            label = new GameObject("Title", typeof(RectTransform), typeof(Text));
            Undo.RegisterCreatedObjectUndo(label, "Create Title");
            label.transform.SetParent(go.transform, false);
        }
        var lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 1f);
        lrt.anchorMax = new Vector2(0.5f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0f, -40f);
        lrt.sizeDelta = new Vector2(1200f, 120f);

        var txt = label.GetComponent<Text>();
        txt.text = "RESOLUTION OVERLAY (DEBUG)";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 48;
        txt.color = Color.black;

        // 8) Disable any gate component that might hide it (by name, no type dependency)
        var gate = go.GetComponent("CaseFlowGateMB");
        if (gate != null)
        {
            // Disable the component to eliminate gating during this reveal
            var comp = (Behaviour)gate;
            comp.enabled = false;
            Debug.LogWarning("[ResolutionUI] Disabled CaseFlowGateMB temporarily on ResolutionRoot.");
        }

        // 9) Activate + bring to top of sibling order
        if (!go.activeSelf) go.SetActive(true);
        go.transform.SetAsLastSibling();

        Selection.activeGameObject = go;
        EditorUtility.SetDirty(go);
        Debug.Log("[ResolutionUI] Revealed ResolutionRoot at top (sortingOrder=9000, ScreenSpaceOverlay).");
    }
}
#endif

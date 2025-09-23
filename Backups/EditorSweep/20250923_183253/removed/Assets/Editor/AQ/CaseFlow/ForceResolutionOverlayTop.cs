#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

static class AQ_ForceResolutionOverlayTop
{
    [MenuItem("AQ/CaseFlow/Force Resolution Overlay TOP (override sorting)")]
    static void ForceTop()
    {
        var go = GameObject.Find("ResolutionRoot");
        if (go == null)
        {
            Debug.LogError("[ResolutionUI] 'ResolutionRoot' not found in active scene.");
            return;
        }

        // Ensure active and has a RectTransform
        if (!go.activeSelf) go.SetActive(true);
        if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();

        // Give it its own top-most Canvas so it renders above everything
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null) canvas = Undo.AddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5000;

        // Raycast support
        if (go.GetComponent<GraphicRaycaster>() == null) Undo.AddComponent<GraphicRaycaster>(go);

        // Dim background so it’s obvious
        var img = go.GetComponent<Image>();
        if (img == null) img = Undo.AddComponent<Image>(go);
        if (img.color.a < 0.3f) img.color = new Color(0f, 0f, 0f, 0.63f);

        // Full-screen stretch
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // Absolute top among siblings
        go.transform.SetAsLastSibling();

        EditorUtility.SetDirty(go);
        Selection.activeGameObject = go;
        Debug.Log("[ResolutionUI] Forced overlay to top with its own Canvas (sortingOrder=5000).");
    }
}
#endif

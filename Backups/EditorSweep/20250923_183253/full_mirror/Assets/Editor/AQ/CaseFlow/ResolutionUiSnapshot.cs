#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

static class AQ_ResolutionUiSnapshot
{
    [MenuItem("AQ/CaseFlow/Dump Resolution UI Snapshot")]
    static void Dump()
    {
        var go = GameObject.Find("ResolutionRoot");
        Debug.Log("=== Resolution UI Snapshot ===");

        if (go == null)
        {
            Debug.LogWarning("ResolutionRoot: NOT FOUND");
        }
        else
        {
            Debug.Log($"ResolutionRoot: activeSelf={go.activeSelf}, activeInHierarchy={go.activeInHierarchy}");
            // Parent chain actives and CanvasGroup alpha
            var p = go.transform;
            int depth = 0;
            while (p != null)
            {
                var cg = p.GetComponent<CanvasGroup>();
                string cgInfo = cg ? $"CanvasGroup alpha={cg.alpha}, interactable={cg.interactable}, blocks={cg.blocksRaycasts}" : "no CanvasGroup";
                Debug.Log($"  Parent[{depth}]: {p.name} (active={p.gameObject.activeSelf}) — {cgInfo}");
                p = p.parent;
                depth++;
            }

            var cv = go.GetComponent<Canvas>();
            if (cv)
            {
                Debug.Log($"Canvas: renderMode={cv.renderMode}, overrideSorting={cv.overrideSorting}, sortingOrder={cv.sortingOrder}");
            }
            else
            {
                Debug.LogWarning("Canvas: MISSING on ResolutionRoot");
            }

            var img = go.GetComponent<Image>();
            if (img)
            {
                Debug.Log($"Image: color={img.color}");
            }
            else
            {
                Debug.LogWarning("Image: MISSING on ResolutionRoot");
            }

            var gate = go.GetComponent("CaseFlowGateMB");
            Debug.Log($"Gate component present: {(gate != null)} (disabled during nuclear reveal).");
        }

        // List top-level canvases and sort orders
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Canvases found: {canvases.Length}");
        foreach (var c in canvases)
        {
            string path = GetPath(c.transform);
            Debug.Log($"  Canvas: {path} | mode={c.renderMode} overrideSorting={c.overrideSorting} order={c.sortingOrder} active={c.gameObject.activeInHierarchy}");
        }

        Debug.Log("=== End Snapshot ===");
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif

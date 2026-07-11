using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ForceDialoguePanelRender : EditorWindow
{
    [MenuItem("Tools/Force DialoguePanel Render")]
    static void Fix()
    {
        var panel = GameObject.Find("DialoguePanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Error", "DialoguePanel not found!", "OK");
            return;
        }

        Debug.Log("=== FIXING DIALOGUEPANEL RENDERING ===");

        // 1. Ensure CanvasRenderer exists
        CanvasRenderer cr = panel.GetComponent<CanvasRenderer>();
        if (cr == null)
        {
            cr = panel.AddComponent<CanvasRenderer>();
            Debug.Log("✅ Added CanvasRenderer");
        }

        // 2. Get or add Image component
        Image img = panel.GetComponent<Image>();
        if (img == null)
        {
            img = panel.AddComponent<Image>();
            Debug.Log("✅ Added Image component");
        }

        // 3. Set bright visible color with FULL ALPHA
        img.color = new Color(1f, 0f, 0f, 1f); // Bright red, full opacity
        Debug.Log($"✅ Set color to RED with alpha={img.color.a}");

        // 4. Force set material
        img.material = null; // Use default UI material
        Debug.Log("✅ Reset material to default");

        // 5. Enable raycast target (helps with visibility)
        img.raycastTarget = true;
        Debug.Log("✅ Enabled raycast target");

        // 6. Verify Canvas parent
        Canvas parentCanvas = panel.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("❌ No parent Canvas found!");
        }
        else
        {
            Debug.Log($"✅ Parent Canvas: {parentCanvas.name}");
            Debug.Log($"   Render Mode: {parentCanvas.renderMode}");
            Debug.Log($"   Sort Order: {parentCanvas.sortingOrder}");
        }

        // 7. Force Canvas rebuild
        if (parentCanvas != null)
        {
            Canvas.ForceUpdateCanvases();
            Debug.Log("✅ Forced Canvas rebuild");
        }

        // 8. Check RectTransform
        RectTransform rt = panel.GetComponent<RectTransform>();
        Debug.Log($"✅ RectTransform:");
        Debug.Log($"   Position: {rt.anchoredPosition}");
        Debug.Log($"   Size: {rt.sizeDelta}");
        Debug.Log($"   Anchors: Min={rt.anchorMin}, Max={rt.anchorMax}");

        // 9. Mark dirty and save
        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(img);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.scene);

        Debug.Log("=== COMPLETE ===");
        
        EditorUtility.DisplayDialog("Done!", 
            "DialoguePanel should now render!\n\n" +
            "1. Check Console for details\n" +
            "2. Look at Game view\n" +
            "3. If still not visible, try entering Play mode\n\n" +
            "Should be a BRIGHT RED bar at the bottom!", 
            "OK");
    }
}
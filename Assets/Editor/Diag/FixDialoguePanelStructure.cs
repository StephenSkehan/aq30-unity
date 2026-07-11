using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixDialoguePanelStructure : EditorWindow
{
    [MenuItem("Tools/Fix DialoguePanel Structure")]
    static void FixStructure()
    {
        Debug.Log("=== FIXING DIALOGUEPANEL STRUCTURE ===");

        // Find both objects
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        GameObject demoCanvas = GameObject.Find("DialogDemoCanvas");

        if (dialoguePanel == null)
        {
            EditorUtility.DisplayDialog("Error", "DialoguePanel not found!", "OK");
            return;
        }

        if (demoCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "DialogDemoCanvas not found!", "OK");
            return;
        }

        // Remove Canvas components from DialoguePanel
        Canvas canvas = dialoguePanel.GetComponent<Canvas>();
        if (canvas != null)
        {
            DestroyImmediate(dialoguePanel.GetComponent<GraphicRaycaster>());
            DestroyImmediate(dialoguePanel.GetComponent<CanvasScaler>());
            DestroyImmediate(canvas);
            Debug.Log("✅ Removed Canvas components from DialoguePanel");
        }

        // Ensure DialoguePanel is child of DialogDemoCanvas
        if (dialoguePanel.transform.parent != demoCanvas.transform)
        {
            dialoguePanel.transform.SetParent(demoCanvas.transform, false);
            Debug.Log("✅ Made DialoguePanel child of DialogDemoCanvas");
        }

        // Ensure CanvasRenderer exists
        CanvasRenderer cr = dialoguePanel.GetComponent<CanvasRenderer>();
        if (cr == null)
        {
            cr = dialoguePanel.AddComponent<CanvasRenderer>();
            Debug.Log("✅ Added CanvasRenderer");
        }

        // Ensure Image component exists with visible color
        Image img = dialoguePanel.GetComponent<Image>();
        if (img == null)
        {
            img = dialoguePanel.AddComponent<Image>();
            Debug.Log("✅ Added Image component");
        }

        // Set BRIGHT RED color
        img.color = new Color(1f, 0f, 0f, 1f);
        img.raycastTarget = true;
        Debug.Log("✅ Set RED color with full alpha");

        // Fix RectTransform - Bottom stretch
        RectTransform rt = dialoguePanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(0f, 300f);
        Debug.Log("✅ Fixed RectTransform (bottom-stretch, 300px tall)");

        // Force rebuild
        Canvas.ForceUpdateCanvases();

        // Mark dirty
        EditorUtility.SetDirty(dialoguePanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(dialoguePanel.scene);

        Debug.Log("=== COMPLETE ===");
        Debug.Log($"Parent Canvas: {dialoguePanel.GetComponentInParent<Canvas>().name}");

        EditorUtility.DisplayDialog("Fixed!", 
            "DialoguePanel structure is now correct!\n\n" +
            "You should see a BRIGHT RED bar at the bottom of Game view.\n\n" +
            "If you don't see it, click OK and check the Game tab.", 
            "OK");
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CopyTestPanelToDialogue : EditorWindow
{
    [MenuItem("Tools/Copy Test Panel Settings to DialoguePanel")]
    static void CopySettings()
    {
        GameObject testPanel = GameObject.Find("TestPanel");
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");

        if (testPanel == null || dialoguePanel == null)
        {
            EditorUtility.DisplayDialog("Error", "Can't find TestPanel or DialoguePanel!", "OK");
            return;
        }

        Debug.Log("=== COPYING WORKING SETTINGS ===");

        // Get components from TestPanel (we know these work!)
        Image testImg = testPanel.GetComponent<Image>();
        RectTransform testRT = testPanel.GetComponent<RectTransform>();

        // Get/create components on DialoguePanel
        Image dialogueImg = dialoguePanel.GetComponent<Image>();
        if (dialogueImg == null)
        {
            dialogueImg = dialoguePanel.AddComponent<Image>();
        }

        RectTransform dialogueRT = dialoguePanel.GetComponent<RectTransform>();

        // COPY EXACT IMAGE SETTINGS
        dialogueImg.sprite = testImg.sprite;
        dialogueImg.color = Color.red; // Red instead of magenta so we can tell them apart
        dialogueImg.material = testImg.material;
        dialogueImg.raycastTarget = testImg.raycastTarget;
        dialogueImg.type = testImg.type;
        
        Debug.Log($"✅ Copied Image settings (color now RED)");

        // Set DialoguePanel to bottom-stretch (not full screen like test)
        dialogueRT.anchorMin = new Vector2(0f, 0f);
        dialogueRT.anchorMax = new Vector2(1f, 0f);
        dialogueRT.pivot = new Vector2(0.5f, 0f);
        dialogueRT.anchoredPosition = new Vector2(0f, 0f);
        dialogueRT.sizeDelta = new Vector2(0f, 300f);

        Debug.Log($"✅ Set DialoguePanel to bottom-stretch, 300px tall");

        // Ensure DialoguePanel is active
        dialoguePanel.SetActive(true);
        Debug.Log($"✅ DialoguePanel active: {dialoguePanel.activeSelf}");

        // Force Canvas rebuild
        Canvas.ForceUpdateCanvases();

        // Save
        EditorUtility.SetDirty(dialoguePanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(dialoguePanel.scene);

        Debug.Log("=== COMPLETE ===");

        EditorUtility.DisplayDialog("Done!", 
            "Copied TestPanel's working settings to DialoguePanel!\n\n" +
            "DialoguePanel should now show as a RED bar at the bottom.\n\n" +
            "Check Game view now!", 
            "OK");
    }
}
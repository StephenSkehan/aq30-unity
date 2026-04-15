using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CleanUpAndStyleDialogue : EditorWindow
{
    [MenuItem("Tools/Clean Up And Style Dialogue")]
    static void CleanAndStyle()
    {
        Debug.Log("=== CLEANING UP AND STYLING ===");

        // Delete TestPanel (magenta - we don't need it anymore)
        GameObject testPanel = GameObject.Find("TestPanel");
        if (testPanel != null)
        {
            DestroyImmediate(testPanel);
            Debug.Log("✅ Deleted TestPanel");
        }

        // Find both DialoguePanels
        GameObject[] allDialoguePanels = GameObject.FindObjectsOfType<GameObject>();
        GameObject rebuiltPanel = null;
        GameObject prefabInstance = null;

        foreach (var obj in allDialoguePanels)
        {
            if (obj.name == "DialoguePanel")
            {
                // Check if it's the rebuilt one (has DialogueController but might not be prefab instance)
                if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
                {
                    rebuiltPanel = obj;
                }
                else
                {
                    prefabInstance = obj;
                }
            }
            else if (obj.name == "DialoguePanel (1)")
            {
                prefabInstance = obj;
            }
        }

        // Delete the rebuilt panel (keep the prefab instance)
        if (rebuiltPanel != null)
        {
            DestroyImmediate(rebuiltPanel);
            Debug.Log("✅ Deleted rebuilt DialoguePanel (keeping prefab instance)");
        }

        // Rename prefab instance to clean name
        if (prefabInstance != null && prefabInstance.name == "DialoguePanel (1)")
        {
            prefabInstance.name = "DialoguePanel";
            Debug.Log("✅ Renamed DialoguePanel (1) to DialoguePanel");
        }

        // Now style the prefab instance properly
        GameObject panel = GameObject.Find("DialoguePanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Error", "Can't find DialoguePanel to style!", "OK");
            return;
        }

        // Style the background - dark semi-transparent
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg != null)
        {
            panelImg.color = new Color(0f, 0f, 0f, 0.85f); // Dark, mostly opaque
            Debug.Log("✅ Set panel to dark semi-transparent");
        }

        // Fix positioning - bottom stretch, 300px tall
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(0f, 300f);
        Debug.Log("✅ Set proper bottom-stretch positioning");

        // Style Portrait
        Transform portraitTransform = panel.transform.Find("Portrait");
        if (portraitTransform != null)
        {
            Image portraitImg = portraitTransform.GetComponent<Image>();
            if (portraitImg != null)
            {
                portraitImg.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray placeholder
                Debug.Log("✅ Styled Portrait");
            }
        }

        // Style Speaker text
        Transform speakerTransform = panel.transform.Find("Speaker");
        if (speakerTransform != null)
        {
            Text speakerText = speakerTransform.GetComponent<Text>();
            if (speakerText != null)
            {
                speakerText.color = new Color(1f, 0.9f, 0.4f, 1f); // Warm yellow
                speakerText.fontStyle = FontStyle.Bold;
                Debug.Log("✅ Styled Speaker text");
            }
        }

        // Style Body text
        Transform bodyTransform = panel.transform.Find("Body");
        if (bodyTransform != null)
        {
            Text bodyText = bodyTransform.GetComponent<Text>();
            if (bodyText != null)
            {
                bodyText.color = Color.white;
                Debug.Log("✅ Styled Body text");
            }
        }

        // Force rebuild
        Canvas.ForceUpdateCanvases();

        // Save
        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.scene);

        Debug.Log("=== COMPLETE ===");

        EditorUtility.DisplayDialog("Styled!", 
            "Scene cleaned up and DialoguePanel styled!\n\n" +
            "You should now see:\n" +
            "- Clean blue game view background\n" +
            "- Dark dialogue panel at bottom\n" +
            "- Yellow speaker name\n" +
            "- White body text\n" +
            "- Gray portrait placeholder\n\n" +
            "Ready to continue with demo setup!", 
            "OK");
    }
}
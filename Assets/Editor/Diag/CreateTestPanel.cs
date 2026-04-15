using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CreateTestPanel : EditorWindow
{
    [MenuItem("Tools/Create Test Panel")]
    static void CreateTest()
    {
        // Find canvas
        GameObject canvas = GameObject.Find("DialogDemoCanvas");
        if (canvas == null)
        {
            Debug.LogError("DialogDemoCanvas not found!");
            return;
        }

        // Delete old test panel if exists
        GameObject oldTest = GameObject.Find("TestPanel");
        if (oldTest != null)
        {
            DestroyImmediate(oldTest);
        }

        // Create new GameObject
        GameObject testPanel = new GameObject("TestPanel");
        testPanel.transform.SetParent(canvas.transform, false);

        // Add RectTransform
        RectTransform rt = testPanel.AddComponent<RectTransform>();
        
        // Full screen for maximum visibility
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Add Image - BRIGHT MAGENTA
        Image img = testPanel.AddComponent<Image>();
        img.color = new Color(1f, 0f, 1f, 1f); // Magenta - impossible to miss
        img.raycastTarget = true;

        // Add Text so we KNOW it's rendering
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(testPanel.transform, false);
        
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = "TEST PANEL - CAN YOU SEE THIS?";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 48;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        Debug.Log("✅ Created FULL SCREEN MAGENTA test panel with text");
        Debug.Log("   If you can't see this, there's a deeper issue");

        // Save
        EditorUtility.SetDirty(testPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(testPanel.scene);

        EditorUtility.DisplayDialog("Test Panel Created!", 
            "Created a FULL SCREEN MAGENTA panel.\n\n" +
            "Check the Game view RIGHT NOW.\n\n" +
            "Can you see:\n" +
            "- Magenta/pink background?\n" +
            "- White text saying 'TEST PANEL'?\n\n" +
            "If NO, we have a rendering issue with your Unity setup.", 
            "OK");
    }
}
using UnityEngine;
using UnityEditor;
using AQ.App;

public class SetupDialogueDemo : EditorWindow
{
    [MenuItem("Tools/Setup Complete Dialogue Demo")]
    static void Setup()
    {
        Debug.Log("=== SETTING UP DIALOGUE DEMO ===");

        // Find DialoguePanel
        GameObject panel = GameObject.Find("DialoguePanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Error", "DialoguePanel not found!", "OK");
            return;
        }

        // Create DialogueRunner GameObject if it doesn't exist
        GameObject runner = GameObject.Find("DialogueRunner");
        if (runner == null)
        {
            runner = new GameObject("DialogueRunner");
            Debug.Log("✅ Created DialogueRunner GameObject");
        }

        // Add DialogueRunner component
        DialogueRunner runnerComponent = runner.GetComponent<DialogueRunner>();
        if (runnerComponent == null)
        {
            runnerComponent = runner.AddComponent<DialogueRunner>();
            Debug.Log("✅ Added DialogueRunner component");
        }

        // Wire up the Panel reference
        DialogueController controller = panel.GetComponent<DialogueController>();
        if (controller != null)
        {
            runnerComponent.Panel = controller;
            Debug.Log("✅ Wired Panel reference");
        }

        // Note: Graph will be assigned after we create it
        Debug.Log("   Note: Graph will be assigned in next step");

        // Create Start Button for testing
        GameObject canvas = GameObject.Find("DialogDemoCanvas");
        GameObject startButton = GameObject.Find("StartDialogueButton");
        
        if (startButton == null && canvas != null)
        {
            startButton = new GameObject("StartDialogueButton");
            startButton.transform.SetParent(canvas.transform, false);

            RectTransform rt = startButton.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.8f);
            rt.anchorMax = new Vector2(0.5f, 0.8f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(200f, 50f);

            UnityEngine.UI.Button btn = startButton.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image btnImg = startButton.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(startButton.transform, false);
            
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "START DIALOGUE";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;

            Debug.Log("✅ Created Start Dialogue button");
        }

        // Save
        EditorUtility.SetDirty(runner);
        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(runner.scene);

        Debug.Log("=== SETUP COMPLETE ===");

        EditorUtility.DisplayDialog("Setup Complete!", 
            "Dialogue demo structure created!\n\n" +
            "Created:\n" +
            "✅ DialogueRunner GameObject\n" +
            "✅ Start Dialogue button\n" +
            "✅ Wired up references\n\n" +
            "Next: Create demo CaseGraph content!", 
            "OK");
    }
}
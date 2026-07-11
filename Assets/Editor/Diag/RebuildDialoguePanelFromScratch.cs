using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RebuildDialoguePanelFromScratch : EditorWindow
{
    [MenuItem("Tools/Rebuild DialoguePanel From Scratch")]
    static void Rebuild()
    {
        GameObject canvas = GameObject.Find("DialogDemoCanvas");
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "DialogDemoCanvas not found!", "OK");
            return;
        }

        Debug.Log("=== REBUILDING DIALOGUEPANEL FROM SCRATCH ===");

        // DELETE old DialoguePanel completely
        GameObject oldPanel = GameObject.Find("DialoguePanel");
        if (oldPanel != null)
        {
            DestroyImmediate(oldPanel);
            Debug.Log("✅ Deleted old DialoguePanel");
        }

        // CREATE FRESH DialoguePanel (like TestPanel that works!)
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(canvas.transform, false);
        Debug.Log("✅ Created fresh DialoguePanel GameObject");

        // Add RectTransform - BOTTOM STRETCH
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(0f, 300f);
        Debug.Log("✅ Added RectTransform (bottom-stretch, 300px)");

        // Add Image - BRIGHT RED
        Image img = panel.AddComponent<Image>();
        img.color = Color.red;
        img.raycastTarget = true;
        Debug.Log("✅ Added Image (RED)");

        // CREATE CHILDREN - Portrait
        GameObject portrait = new GameObject("Portrait");
        portrait.transform.SetParent(panel.transform, false);
        
        RectTransform portraitRT = portrait.AddComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0f, 0.5f);
        portraitRT.anchorMax = new Vector2(0f, 0.5f);
        portraitRT.pivot = new Vector2(0.5f, 0.5f);
        portraitRT.anchoredPosition = new Vector2(150f, 0f);
        portraitRT.sizeDelta = new Vector2(200f, 200f);
        
        Image portraitImg = portrait.AddComponent<Image>();
        portraitImg.color = Color.white;
        
        Debug.Log("✅ Added Portrait");

        // CREATE CHILDREN - Speaker
        GameObject speaker = new GameObject("Speaker");
        speaker.transform.SetParent(panel.transform, false);
        
        RectTransform speakerRT = speaker.AddComponent<RectTransform>();
        speakerRT.anchorMin = new Vector2(0.2f, 0.7f);
        speakerRT.anchorMax = new Vector2(0.95f, 0.95f);
        speakerRT.pivot = new Vector2(0.5f, 0.5f);
        speakerRT.anchoredPosition = Vector2.zero;
        speakerRT.sizeDelta = Vector2.zero;
        
        Text speakerText = speaker.AddComponent<Text>();
        speakerText.text = "Speaker Name";
        speakerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speakerText.fontSize = 28;
        speakerText.color = Color.yellow;
        speakerText.alignment = TextAnchor.MiddleLeft;
        
        Debug.Log("✅ Added Speaker");

        // CREATE CHILDREN - Body
        GameObject body = new GameObject("Body");
        body.transform.SetParent(panel.transform, false);
        
        RectTransform bodyRT = body.AddComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.2f, 0.15f);
        bodyRT.anchorMax = new Vector2(0.95f, 0.65f);
        bodyRT.pivot = new Vector2(0.5f, 0.5f);
        bodyRT.anchoredPosition = Vector2.zero;
        bodyRT.sizeDelta = Vector2.zero;
        
        Text bodyText = body.AddComponent<Text>();
        bodyText.text = "Dialogue text will appear here...";
        bodyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyText.fontSize = 22;
        bodyText.color = Color.white;
        bodyText.alignment = TextAnchor.UpperLeft;
        
        Debug.Log("✅ Added Body");

        // CREATE CHILDREN - AdvanceArea (button)
        GameObject advanceBtn = new GameObject("AdvanceArea");
        advanceBtn.transform.SetParent(panel.transform, false);
        
        RectTransform advanceRT = advanceBtn.AddComponent<RectTransform>();
        advanceRT.anchorMin = new Vector2(0f, 0f);
        advanceRT.anchorMax = new Vector2(1f, 1f);
        advanceRT.pivot = new Vector2(0.5f, 0.5f);
        advanceRT.anchoredPosition = Vector2.zero;
        advanceRT.sizeDelta = Vector2.zero;
        
        Button advanceButton = advanceBtn.AddComponent<Button>();
        Image advanceBtnImg = advanceBtn.AddComponent<Image>();
        advanceBtnImg.color = new Color(0f, 0f, 0f, 0f); // Invisible but clickable
        
        Debug.Log("✅ Added AdvanceArea button");

        // Add DialogueController component
        var controller = panel.AddComponent<AQ.App.DialogueController>();
        controller.Speaker = speakerText;
        controller.Body = bodyText;
        controller.portraitImage = portraitImg;
        controller.AdvanceArea = advanceButton;
        
        Debug.Log("✅ Added DialogueController with references");

        // Force rebuild
        Canvas.ForceUpdateCanvases();

        // Save
        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.scene);

        Debug.Log("=== REBUILD COMPLETE ===");
        Debug.Log($"DialoguePanel active: {panel.activeSelf}");
        Debug.Log($"Parent: {panel.transform.parent.name}");

        EditorUtility.DisplayDialog("Rebuilt!", 
            "DialoguePanel has been completely rebuilt from scratch!\n\n" +
            "It's built exactly like TestPanel that works.\n\n" +
            "You should now see:\n" +
            "- MAGENTA screen (TestPanel)\n" +
            "- RED bar at bottom (DialoguePanel)\n\n" +
            "Check Game view NOW!", 
            "OK");
    }
}
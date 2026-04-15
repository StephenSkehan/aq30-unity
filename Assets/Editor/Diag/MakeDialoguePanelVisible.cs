using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class MakeDialoguePanelVisible : EditorWindow
{
    [MenuItem("Tools/Make DialoguePanel Visible")]
    static void Fix()
    {
        // Find DialoguePanel in scene
        var panel = GameObject.Find("DialoguePanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Error", "DialoguePanel not found in scene!", "OK");
            return;
        }

        // Get or add Image component
        Image img = panel.GetComponent<Image>();
        if (img == null)
        {
            img = panel.AddComponent<Image>();
            Debug.Log("✅ Added Image component");
        }

        // Set bright color
        img.color = Color.red;
        Debug.Log("✅ Set color to RED");

        // Mark scene dirty
        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.scene);

        EditorUtility.DisplayDialog("Success!", 
            "DialoguePanel should now be BRIGHT RED and visible in Game view!\n\n" +
            "Check the Game tab now.", 
            "OK");
    }
}
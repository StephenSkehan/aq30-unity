using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixDialoguePanelPrefab : EditorWindow
{
    [MenuItem("Tools/Fix DialoguePanel Prefab")]
    static void FixPrefab()
    {
        // Find the prefab
        string[] guids = AssetDatabase.FindAssets("DialoguePanel t:Prefab");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Could not find DialoguePanel prefab!", "OK");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not load prefab!", "OK");
            return;
        }

        // Open prefab for editing
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        bool madeChanges = false;

        // FIX 1: Remove Canvas components from root
        Canvas canvas = prefabRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            DestroyImmediate(canvas);
            Debug.Log("✅ Removed Canvas component from root");
            madeChanges = true;
        }

        CanvasScaler scaler = prefabRoot.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            DestroyImmediate(scaler);
            Debug.Log("✅ Removed CanvasScaler component from root");
            madeChanges = true;
        }

        GraphicRaycaster raycaster = prefabRoot.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            DestroyImmediate(raycaster);
            Debug.Log("✅ Removed GraphicRaycaster component from root");
            madeChanges = true;
        }

        // FIX 2: Fix Portrait position
        Transform portrait = prefabRoot.transform.Find("DialoguePanel/Portrait");
        if (portrait != null)
        {
            RectTransform rt = portrait.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Set to middle-left anchor
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                
                // Position on left side
                rt.anchoredPosition = new Vector2(150f, 0f);
                rt.sizeDelta = new Vector2(200f, 200f);

                Debug.Log("✅ Fixed Portrait position");
                madeChanges = true;
            }
        }

        // FIX 3: Flatten hierarchy if nested
        Transform childPanel = prefabRoot.transform.Find("DialoguePanel");
        if (childPanel != null && childPanel != prefabRoot.transform)
        {
            // Move all children of nested DialoguePanel to root
            while (childPanel.childCount > 0)
            {
                Transform child = childPanel.GetChild(0);
                child.SetParent(prefabRoot.transform, false);
            }

            // Copy DialogueController component
            var childController = childPanel.GetComponent<AQ.App.DialogueController>();
            if (childController != null)
            {
                var rootController = prefabRoot.GetComponent<AQ.App.DialogueController>();
                if (rootController == null)
                {
                    rootController = prefabRoot.AddComponent<AQ.App.DialogueController>();
                }
                
                // Copy field values
                EditorUtility.CopySerialized(childController, rootController);
                Debug.Log("✅ Moved DialogueController to root");
            }

            // Copy Image component
            var childImage = childPanel.GetComponent<Image>();
            if (childImage != null)
            {
                var rootImage = prefabRoot.GetComponent<Image>();
                if (rootImage == null)
                {
                    rootImage = prefabRoot.AddComponent<Image>();
                }
                EditorUtility.CopySerialized(childImage, rootImage);
                Debug.Log("✅ Moved Image to root");
            }

            // Delete the nested panel
            DestroyImmediate(childPanel.gameObject);
            Debug.Log("✅ Removed nested DialoguePanel");
            madeChanges = true;
        }

        // FIX 4: Set proper root RectTransform for UI panel
        RectTransform rootRT = prefabRoot.GetComponent<RectTransform>();
        if (rootRT != null)
        {
            // Bottom-stretch anchoring
            rootRT.anchorMin = new Vector2(0f, 0f);
            rootRT.anchorMax = new Vector2(1f, 0f);
            rootRT.pivot = new Vector2(0.5f, 0f);
            rootRT.anchoredPosition = new Vector2(0f, 0f);
            rootRT.sizeDelta = new Vector2(0f, 300f); // 300px tall

            Debug.Log("✅ Fixed root RectTransform");
            madeChanges = true;
        }

        // Save changes
        if (madeChanges)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success!", 
                "DialoguePanel prefab has been fixed!\n\n" +
                "Changes made:\n" +
                "- Removed Canvas components\n" +
                "- Fixed Portrait position\n" +
                "- Flattened hierarchy\n" +
                "- Set proper anchoring\n\n" +
                "You can now use it in your scene!", 
                "OK");
        }
        else
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            EditorUtility.DisplayDialog("Info", "No changes needed - prefab looks good!", "OK");
        }
    }
}
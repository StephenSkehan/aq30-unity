using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class CreateDragLayer : EditorWindow
{
    [MenuItem("Tools/Board/1. Create DragLayer")]
    static void Execute()
    {
        var canvasBoard = GameObject.Find("Canvas_Board");
        if (!canvasBoard)
        {
            EditorUtility.DisplayDialog("Error", "Canvas_Board not found in scene!", "OK");
            return;
        }

        // Check if DragLayer already exists
        Transform existing = canvasBoard.transform.Find("DragLayer");
        if (existing)
        {
            Debug.Log("[CreateDragLayer] DragLayer already exists!");
            EditorUtility.DisplayDialog("Already Exists", "DragLayer already exists under Canvas_Board", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create DragLayer
        GameObject dragLayer = new GameObject("DragLayer");
        dragLayer.transform.SetParent(canvasBoard.transform, false);
        
        // Add RectTransform (automatically added for UI objects)
        RectTransform rt = dragLayer.GetComponent<RectTransform>();
        if (!rt)
            rt = dragLayer.AddComponent<RectTransform>();
        
        // Stretch to fill parent
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // CRITICAL: Move to end so it renders on top
        dragLayer.transform.SetAsLastSibling();

        // Add Canvas override for guaranteed top rendering
        Canvas cv = dragLayer.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 1000;
        
        // Add GraphicRaycaster (standard for UI canvases)
        dragLayer.AddComponent<GraphicRaycaster>();

        // Optional: Add CanvasGroup with blocksRaycasts=false
        CanvasGroup cg = dragLayer.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("[CreateDragLayer] ✓ DragLayer created successfully under Canvas_Board");
        EditorUtility.DisplayDialog("Success", "DragLayer created successfully!\n\nIt's an empty GameObject that will hold icons during drag operations.", "OK");
        
        Selection.activeGameObject = dragLayer;
    }
}
#endif